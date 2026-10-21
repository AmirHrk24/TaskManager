using System.Text;

namespace TaskManager.Api.Logging;

public interface IRequestLogWriter
{
    Task WriteAsync(string entry, CancellationToken ct = default);
}
public sealed class FileRequestLogWriter : IRequestLogWriter
{
    private readonly string _directory;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public FileRequestLogWriter(string directory)
    {
        _directory = directory;
        Directory.CreateDirectory(_directory);
    }

    public async Task WriteAsync(string entry, CancellationToken ct = default)
    {
        var path = Path.Combine(_directory, $"http-{DateTime.UtcNow:yyyy-MM-dd}.log");

        await _lock.WaitAsync(ct);
        try
        {
            await File.AppendAllTextAsync(path, entry + Environment.NewLine, Encoding.UTF8, ct);
        }
        finally
        {
            _lock.Release();
        }
    }
}
