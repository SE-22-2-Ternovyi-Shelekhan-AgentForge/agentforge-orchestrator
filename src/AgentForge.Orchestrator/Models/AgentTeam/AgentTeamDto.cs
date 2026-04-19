namespace AgentForge.Orchestrator.Models
{
    public class AgentTeamDto
    {
        public Guid AgentTeamId { get; set; }
        public string Name { get; set; }
        public List<AgentDto> Agents { get; set; } = new List<AgentDto>();
        public DateTime CreatedAt { get; set; }
    }
}
