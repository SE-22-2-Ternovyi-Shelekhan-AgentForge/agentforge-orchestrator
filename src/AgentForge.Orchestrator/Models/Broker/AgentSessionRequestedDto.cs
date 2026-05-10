using System.Text.Json.Serialization;

namespace AgentForge.Orchestrator.Models.Broker;

public class AgentSessionRequestedDto
{
    [JsonPropertyName("session_id")]      public Guid SessionId { get; set; }
    [JsonPropertyName("conversation_id")] public Guid ConversationId { get; set; }
    [JsonPropertyName("user_prompt")]     public string UserPrompt { get; set; } = "";
    [JsonPropertyName("history")]         public List<ContextMessageDto> History { get; set; } = new();
    [JsonPropertyName("team")]            public TeamConfigDto Team { get; set; } = new();
}

public class ContextMessageDto
{
    [JsonPropertyName("role")]       public string Role { get; set; } = "";
    [JsonPropertyName("content")]    public string Content { get; set; } = "";
    [JsonPropertyName("agent_role")] public string? AgentRole { get; set; }
    [JsonPropertyName("timestamp")]  public DateTime? Timestamp { get; set; }
}

public class TeamConfigDto
{
    [JsonPropertyName("supervisor_prompt")] public string? SupervisorPrompt { get; set; }
    [JsonPropertyName("agents")]            public List<AgentConfigDto> Agents { get; set; } = new();
    [JsonPropertyName("max_iterations")]    public int MaxIterations { get; set; } = 10;
}

public class AgentConfigDto
{
    [JsonPropertyName("role")]          public string Role { get; set; } = "";
    [JsonPropertyName("system_prompt")] public string SystemPrompt { get; set; } = "";
    [JsonPropertyName("model")]         public string? Model { get; set; }
    [JsonPropertyName("temperature")]   public double? Temperature { get; set; }
    [JsonPropertyName("tools")]         public List<string> Tools { get; set; } = new();
}
