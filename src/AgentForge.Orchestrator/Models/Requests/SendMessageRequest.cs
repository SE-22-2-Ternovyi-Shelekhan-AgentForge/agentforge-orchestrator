using System.ComponentModel.DataAnnotations;

namespace AgentForge.Orchestrator.Models
{
    public class SendMessageRequest
    {
        [Required]
        public Guid ConversationId { get; set; }
        public Guid? AgentId { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        [MaxLength(100)]
        public string SenderName { get; set; }
    }
}
