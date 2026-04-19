using System.ComponentModel.DataAnnotations;

namespace AgentForge.Orchestrator.Models
{
    public class CreateConversationRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }
    }
}
