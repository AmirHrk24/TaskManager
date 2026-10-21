
using System.Text;
using System.Text.Json;
using TaskManager.Infrastructure.Logging;

namespace TaskManager.Api.Logging;

public interface IRequestLogWriter
{
    Task WriteAsync(
        TaskManagerLog log,
        CancellationToken ct = default);
}

public sealed class FileRequestLogWriter
{
    private readonly string _directory;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    public FileRequestLogWriter(string directory)
    {
        _directory = directory;
        Directory.CreateDirectory(_directory);
    }

    public async Task WriteAsync(
        TaskManagerLog log,
        CancellationToken ct = default)
    {
        var path = Path.Combine(
            _directory,
            $"http-{DateTime.UtcNow:yyyy-MM-dd}.log");

        var entry = JsonSerializer.Serialize(log, JsonOptions);

        await _lock.WaitAsync(ct);

        try
        {
            await File.AppendAllTextAsync(
                path,
                entry + Environment.NewLine,
                Encoding.UTF8,
                ct);
        }
        finally
        {
            _lock.Release();
        }
    }
}
