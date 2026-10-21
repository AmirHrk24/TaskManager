
using Microsoft.Extensions.Logging;
using TaskManager.Infrastructure.Logging;

namespace TaskManager.Api.Logging;

public sealed class CompositeRequestLogWriter : IRequestLogWriter
{
    private readonly FileRequestLogWriter _fileWriter;
    private readonly ElasticsearchLogWriter _elasticsearchLogWriter;
    private readonly ILogger<CompositeRequestLogWriter> _logger;

    public CompositeRequestLogWriter(
        FileRequestLogWriter fileWriter,
        ElasticsearchLogWriter elasticsearchLogWriter,
        ILogger<CompositeRequestLogWriter> logger)
    {
        _fileWriter = fileWriter;
        _elasticsearchLogWriter = elasticsearchLogWriter;
        _logger = logger;
    }

    public async Task WriteAsync(
        TaskManagerLog log,
        CancellationToken ct = default)
    {
        try
        {
            await _fileWriter.WriteAsync(log, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to write HTTP log to file. TraceId: {TraceId}",
                log.TraceId);
        }

        try
        {
            await _elasticsearchLogWriter.WriteAsync(log, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to write HTTP log to Elasticsearch. TraceId: {TraceId}",
                log.TraceId);
        }
    }
}
