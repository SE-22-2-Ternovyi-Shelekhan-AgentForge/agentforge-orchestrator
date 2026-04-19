namespace AgentForge.Orchestrator.Models
{
    public class AgentTeamDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Agent> Agents { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
