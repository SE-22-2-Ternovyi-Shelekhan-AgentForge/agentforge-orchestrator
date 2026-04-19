namespace AgentForge.Orchestrator.Models
{
    public class Agent
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string SystemPrompt { get; set; }
        public Guid TeamId { get; set; }
    }
}
