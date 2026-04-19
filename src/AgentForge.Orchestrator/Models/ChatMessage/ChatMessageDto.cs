namespace AgentForge.Orchestrator.Models
{
    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public string SenderName { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Guid TeamId { get; set; }
    }
}
