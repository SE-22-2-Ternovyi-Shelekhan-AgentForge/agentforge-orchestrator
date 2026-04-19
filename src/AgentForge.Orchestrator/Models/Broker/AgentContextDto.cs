namespace AgentForge.Orchestrator.Models
{
    public class AgentContextDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string SystemPrompt { get; set; }
        public string ModelName { get; set; }
        public double Temperature { get; set; }
        public List<string> Tools { get; set; } = new List<string>();
    }
}
