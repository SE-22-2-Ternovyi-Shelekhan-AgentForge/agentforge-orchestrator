using System.Text.Json.Serialization;

namespace AgentForge.Orchestrator.Models.Broker;

public class AgentSessionFailedDto
{
    [JsonPropertyName("session_id")]      public Guid SessionId { get; set; }
    [JsonPropertyName("conversation_id")] public Guid ConversationId { get; set; }
    [JsonPropertyName("error_type")]      public string ErrorType { get; set; } = "";
    [JsonPropertyName("error_message")]   public string ErrorMessage { get; set; } = "";
    [JsonPropertyName("partial_trace")]   public List<TraceEntryDto> PartialTrace { get; set; } = new();
    [JsonPropertyName("failed_at")]       public DateTime FailedAt { get; set; }
}
