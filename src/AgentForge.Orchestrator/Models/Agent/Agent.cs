using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgentForge.Orchestrator.Models
{
    public class Agent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid AgentId { get; set; }

        [Required]
        public Guid AgentTeamId { get; set; }

        [ForeignKey("AgentTeamId")]
        public AgentTeam Team { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Role { get; set; }

        [Required]
        public string SystemPrompt { get; set; }

        [Required]
        [MaxLength(50)]
        public string ModelName { get; set; }

        [Required]
        public double Temperature { get; set; }

        public string Capabilities { get; set; }
    }
}
