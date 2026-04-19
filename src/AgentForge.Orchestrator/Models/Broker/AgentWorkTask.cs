namespace AgentForge.Orchestrator.Models.Broker
{
    public class AgentWorkTask
    {
        public Guid TaskId { get; set; } = Guid.NewGuid();
        public Guid ConversationId { get; set; }
        public Guid TeamId { get; set; }
        public AgentContextDto TargetAgent { get; set; }
        public List<ChatMessageDto> History { get; set; } = new List<ChatMessageDto>();
        public string UserInput { get; set; }
    }
}
