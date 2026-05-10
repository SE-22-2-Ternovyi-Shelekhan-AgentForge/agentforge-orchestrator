using System.Text.Json.Serialization;

namespace AgentForge.Orchestrator.Models.Broker;

public class AgentSessionCompletedDto
{
    [JsonPropertyName("session_id")]       public Guid SessionId { get; set; }
    [JsonPropertyName("conversation_id")]  public Guid ConversationId { get; set; }
    [JsonPropertyName("final_output")]     public string FinalOutput { get; set; } = "";
    [JsonPropertyName("trace")]            public List<TraceEntryDto> Trace { get; set; } = new();
    [JsonPropertyName("tokens_in_total")]  public int? TokensInTotal { get; set; }
    [JsonPropertyName("tokens_out_total")] public int? TokensOutTotal { get; set; }
    [JsonPropertyName("iterations")]       public int Iterations { get; set; }
    [JsonPropertyName("completed_at")]     public DateTime CompletedAt { get; set; }
}

public class TraceEntryDto
{
    [JsonPropertyName("agent_role")]  public string AgentRole { get; set; } = "";
    [JsonPropertyName("output")]      public string Output { get; set; } = "";
    [JsonPropertyName("tools_used")]  public List<string> ToolsUsed { get; set; } = new();
    [JsonPropertyName("tokens_in")]   public int? TokensIn { get; set; }
    [JsonPropertyName("tokens_out")]  public int? TokensOut { get; set; }
}
