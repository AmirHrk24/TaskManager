
using Elastic.Clients.Elasticsearch;

namespace TaskManager.Infrastructure.Logging;

public sealed class ElasticsearchLogWriter
{
    private readonly ElasticsearchClient _client;
    private readonly string _dataStream;

    public ElasticsearchLogWriter(
        ElasticsearchClient client,
        string dataStream = "logs-taskmanager-api-default")
    {
        _client = client;
        _dataStream = dataStream;
    }

    public async Task WriteAsync(
        TaskManagerLog log,
        CancellationToken ct = default)
    {
        var response = await _client.IndexAsync(
            log,
            request => request
                .Index(_dataStream)
                .OpType(Elastic.Clients.Elasticsearch.OpType.Create),
            ct);

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException(
                $"Failed to write log to Elasticsearch: {response.DebugInformation}");
        }
    }
}
