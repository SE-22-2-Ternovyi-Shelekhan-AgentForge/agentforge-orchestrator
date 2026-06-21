using System.ComponentModel.DataAnnotations;

namespace AgentForge.Orchestrator.Models
{
    public class RenameConversationRequest
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }
    }
}
