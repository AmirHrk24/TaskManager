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
        // حفظ لاگ فایل فعلی
        await _fileWriter.WriteAsync(entry, ct);

        // ارسال همان لاگ به Serilog و Elasticsearch
        Serilog.Log.Information(
            "HTTP request/response: {RequestLogEntry}",
            entry);
    }
}