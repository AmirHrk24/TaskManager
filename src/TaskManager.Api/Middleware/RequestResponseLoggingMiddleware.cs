using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using TaskManager.Api.Logging;

namespace TaskManager.Api.Middleware;

/// <summary>Writes every incoming request and outgoing response to a log file.</summary>
public sealed class RequestResponseLoggingMiddleware(
    RequestDelegate next,
    IRequestLogWriter writer,
    ILogger<RequestResponseLoggingMiddleware> logger)
{
    private const int MaxBodyLength = 4096;

    // Never write secrets to the log file.
    private static readonly Regex SensitiveJson = new(
        "\"(password|token|accessToken|passwordHash)\"\\s*:\\s*\"(?:\\\\.|[^\"\\\\])*\"",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task InvokeAsync(HttpContext context)
    {
        // Swagger UI / OpenAPI document are static docs: no need to log them.
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
                using var reader = new StreamReader(responseBuffer, Encoding.UTF8, false, 1024, leaveOpen: true);
                responseBody = await reader.ReadToEndAsync();
            }

            // Send the buffered response to the real client.
            responseBuffer.Position = 0;
            await responseBuffer.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

            await WriteLogAsync(context, requestTime, stopwatch.ElapsedMilliseconds, requestBody, responseBody);
        }
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        if (!IsLoggableContentType(request.ContentType) || request.ContentLength == 0)
            return string.Empty;

        request.EnableBuffering();
        using var reader = new StreamReader(request.Body, Encoding.UTF8, false, 1024, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }

    private static bool IsLoggableContentType(string? contentType)
        => !string.IsNullOrEmpty(contentType)
           && (contentType.Contains("json", StringComparison.OrdinalIgnoreCase)
               || contentType.Contains("text", StringComparison.OrdinalIgnoreCase));

    private static string Sanitize(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return "-";

        var masked = SensitiveJson.Replace(body, "\"$1\":\"***\"");
        return masked.Length > MaxBodyLength
            ? masked[..MaxBodyLength] + "...[truncated]"
            : masked;
    }

    private async Task WriteLogAsync(
        HttpContext context, DateTime requestTime, long elapsedMs, string requestBody, string responseBody)
    {
        try
        {
            var user = context.User.Identity?.IsAuthenticated == true
                ? $"{context.User.FindFirst("sub")?.Value} ({context.User.FindFirst("role")?.Value})"
                : "anonymous";

            var sb = new StringBuilder();
            sb.AppendLine($"=== {context.TraceIdentifier} ===");
            sb.AppendLine($"[{requestTime:yyyy-MM-dd HH:mm:ss.fff} UTC] REQUEST  {context.Request.Method} " +
                          $"{context.Request.Path}{context.Request.QueryString} | User: {user} | " +
                          $"IP: {context.Connection.RemoteIpAddress}");
            sb.AppendLine($"  Body: {Sanitize(requestBody)}");
            sb.AppendLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC] RESPONSE {context.Response.StatusCode} | " +
                          $"Elapsed: {elapsedMs} ms");
            sb.AppendLine($"  Body: {Sanitize(responseBody)}");

            await writer.WriteAsync(sb.ToString(), CancellationToken.None);
        }
        catch (Exception ex)
        {
            // Logging must never break the request.
            logger.LogError(ex, "Failed to write the HTTP log entry.");
        }
    }
}
