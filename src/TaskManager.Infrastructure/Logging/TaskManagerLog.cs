
using System.Text.Json.Serialization;

namespace TaskManager.Infrastructure.Logging;

public class TaskManagerLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("@timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string Level { get; set; } = "Information";

    public string Message { get; set; } = string.Empty;

    public string? Exception { get; set; }

    public string? HttpMethod { get; set; }

    public string? RequestPath { get; set; }

    public int? StatusCode { get; set; }

    public long? ElapsedMilliseconds { get; set; }

    public string? TraceId { get; set; }

    public TaskManagerLogUser? User { get; set; }

    public string? ClientIp { get; set; }

    public string? RequestBody { get; set; }

    public string? ResponseBody { get; set; }
}

public class TaskManagerLogUser
{
    public string? Id { get; set; }

    public string? Name { get; set; }

    public string[]? Roles { get; set; }
}
