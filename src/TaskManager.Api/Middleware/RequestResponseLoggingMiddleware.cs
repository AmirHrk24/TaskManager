
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using TaskManager.Api.Logging;
using TaskManager.Infrastructure.Logging;

namespace TaskManager.Api.Middleware;

public sealed class RequestResponseLoggingMiddleware(
    RequestDelegate next,
    IRequestLogWriter writer,
    ILogger<RequestResponseLoggingMiddleware> logger)
{
    private const int MaxBodyLength = 4096;

    private static readonly Regex SensitiveJson = new(
        "\"(password|token|accessToken|passwordHash)\"\\s*:\\s*\"(?:\\\\.|[^\"\\\\])*\"",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/openapi"))
        {
            await next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestTime = DateTime.UtcNow;
        var requestBody = await ReadRequestBodyAsync(context.Request);

        var originalBody = context.Response.Body;

        await using var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            var responseBody = string.Empty;

            if (IsLoggableContentType(context.Response.ContentType))
            {
                responseBuffer.Position = 0;

                using var reader = new StreamReader(
                    responseBuffer,
                    Encoding.UTF8,
                    false,
                    1024,
                    leaveOpen: true);

                responseBody = await reader.ReadToEndAsync();
            }

            responseBuffer.Position = 0;
            await responseBuffer.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

            await WriteLogAsync(
                context,
                requestTime,
                stopwatch.ElapsedMilliseconds,
                requestBody,
                responseBody);
        }
    }

    private static async Task<string> ReadRequestBodyAsync(
        HttpRequest request)
    {
        if (!IsLoggableContentType(request.ContentType) ||
            request.ContentLength == 0)
        {
            return string.Empty;
        }

        request.EnableBuffering();

        using var reader = new StreamReader(
            request.Body,
            Encoding.UTF8,
            false,
            1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        return Sanitize(body);
    }

    private static bool IsLoggableContentType(string? contentType)
        => !string.IsNullOrEmpty(contentType)
           && (contentType.Contains(
                   "json",
                   StringComparison.OrdinalIgnoreCase)
               || contentType.Contains(
                   "text",
                   StringComparison.OrdinalIgnoreCase));

    private static string Sanitize(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return "-";
        }

        var masked = SensitiveJson.Replace(body, "\"$1\":\"***\"");

        return masked.Length > MaxBodyLength
            ? masked[..MaxBodyLength] + "...[truncated]"
            : masked;
    }

    private async Task WriteLogAsync(
        HttpContext context,
        DateTime requestTime,
        long elapsedMs,
        string requestBody,
        string responseBody)
    {
        try
        {
            var isAuthenticated =
                context.User.Identity?.IsAuthenticated == true;

            TaskManagerLogUser? user = isAuthenticated
                ? new TaskManagerLogUser
                {
                    Id = context.User.FindFirst("sub")?.Value,
                    Name = context.User.FindFirst("name")?.Value
                           ?? context.User.Identity?.Name,
                    Roles = context.User.FindAll("role")
                        .Select(claim => claim.Value)
                        .ToArray()
                }
                : null;

            var log = new TaskManagerLog
            {
                Timestamp = requestTime,
                Level = context.Response.StatusCode >= 500
                    ? "Error"
                    : context.Response.StatusCode >= 400
                        ? "Warning"
                        : "Information",
                Message =
                    $"{context.Request.Method} {context.Request.Path} completed with status {context.Response.StatusCode}",
                HttpMethod = context.Request.Method,
                RequestPath =
                    context.Request.Path + context.Request.QueryString,
                StatusCode = context.Response.StatusCode,
                ElapsedMilliseconds = elapsedMs,
                TraceId = Activity.Current?.TraceId.ToString()
                          ?? context.TraceIdentifier,
                User = user,
                ClientIp =
                    context.Connection.RemoteIpAddress?.ToString(),
                RequestBody = requestBody,
                ResponseBody = Sanitize(responseBody)
            };

            await writer.WriteAsync(log, CancellationToken.None);
        }
        catch (Exception ex)
        {
            // Logging must never break the HTTP request.
            logger.LogError(
                ex,
                "Failed to write the HTTP log entry.");
        }
    }
}
