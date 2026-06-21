namespace AgentForge.Orchestrator.Models.Broker
{
    public class AgentSessionTraceDto
    {
        public Guid SessionId { get; set; }
        public Guid ConversationId { get; set; }
        public List<TraceStepDto> Trace { get; set; } = new();
        public int? TokensInTotal { get; set; }
        public int? TokensOutTotal { get; set; }
        public int Iterations { get; set; }
        public DateTime CompletedAt { get; set; }
        public string? ErrorType { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class TraceStepDto
    {
        public string AgentRole { get; set; } = "";
        public string Output { get; set; } = "";
        public List<string> ToolsUsed { get; set; } = new();
        public int? TokensIn { get; set; }
        public int? TokensOut { get; set; }
    }
}
