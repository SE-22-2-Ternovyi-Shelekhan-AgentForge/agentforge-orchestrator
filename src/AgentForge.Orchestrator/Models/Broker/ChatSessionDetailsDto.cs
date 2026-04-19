namespace AgentForge.Orchestrator.Models.Broker
{
    public class ChatSessionDetailsDto
    {
        public Guid ConversationId { get; set; }
        public string Title { get; set; }
        public Guid? TeamId { get; set; }
        public string? TeamName { get; set; }
        public List<AgentDto> Agents { get; set; } = new List<AgentDto>();
        public List<ChatMessageDto> Messages { get; set; } = new List<ChatMessageDto>();
    }
}
