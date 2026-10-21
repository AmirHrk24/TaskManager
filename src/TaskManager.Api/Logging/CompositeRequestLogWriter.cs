namespace TaskManager.Api.Logging;

public sealed class CompositeRequestLogWriter : IRequestLogWriter
{
    private readonly FileRequestLogWriter _fileWriter;

    public CompositeRequestLogWriter(FileRequestLogWriter fileWriter)
    {
        _fileWriter = fileWriter;
    }

    public async Task WriteAsync(
        string entry,
        CancellationToken ct = default)
    {
      
        await _fileWriter.WriteAsync(entry, ct);

        Serilog.Log.Information(
            "HTTP request/response: {RequestLogEntry}",
            entry);
    }
}