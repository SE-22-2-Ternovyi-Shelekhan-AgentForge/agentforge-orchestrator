using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentForge.Orchestrator.Models.Broker;

public class AgentEventOccurredDto
{
    [JsonPropertyName("session_id")]      public Guid SessionId { get; set; }
    [JsonPropertyName("conversation_id")] public Guid ConversationId { get; set; }
    [JsonPropertyName("event_type")]      public string EventType { get; set; } = "";
    [JsonPropertyName("agent_role")]      public string? AgentRole { get; set; }
    [JsonPropertyName("payload")]         public JsonElement Payload { get; set; }
    [JsonPropertyName("timestamp")]       public DateTime Timestamp { get; set; }
}
