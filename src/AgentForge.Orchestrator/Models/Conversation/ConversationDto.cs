namespace AgentForge.Orchestrator.Models
{
    public class ConversationDto
    {
        public Guid ConversationId { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? AgentTeamId { get; set; }
        public string? TeamName { get; set; }
    }
}
