using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgentForge.Orchestrator.Models
{
    public class AgentTeam
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid AgentTeamId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string? SupervisorPrompt { get; set; }

        // Team-level orchestration limits. Null → worker defaults (rounds 2, iterations 10).
        public int? MaxRounds { get; set; }
        public int? MaxIterations { get; set; }

        public List<Agent> Agents { get; set; } = new List<Agent>();
        public List<Conversation> Conversations { get; set; } = new List<Conversation>();
    }
}
