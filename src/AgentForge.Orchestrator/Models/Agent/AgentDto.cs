namespace AgentForge.Orchestrator.Models
{
    public class AgentDto
    {
        public Guid AgentId { get; set; }
        public Guid AgentTeamId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string SystemPrompt { get; set; }
        public string ModelName { get; set; }
        public double Temperature { get; set; }
        public List<string> Capabilities { get; set; } = new List<string>();
        public List<string> Tools { get; set; } = new();
    }
}
