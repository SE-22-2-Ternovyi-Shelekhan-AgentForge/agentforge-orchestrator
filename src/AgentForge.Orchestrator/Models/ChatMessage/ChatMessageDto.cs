namespace AgentForge.Orchestrator.Models
{
    public class ChatMessageDto
    {
        public Guid ChatMessageId { get; set; }
        public Guid ConversationId { get; set; }
        public string Content { get; set; }
        public string Role { get; set; }
        public string SenderName { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
