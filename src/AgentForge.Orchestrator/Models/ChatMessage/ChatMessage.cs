using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgentForge.Orchestrator.Models
{
    public class ChatMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ChatMessageId { get; set; }

        [Required]
        public Guid ConversationId { get; set; }

        [ForeignKey("ConversationId")]
        public Conversation Conversation { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        [MaxLength(50)]
        public string Role { get; set; }

        [MaxLength(100)]
        public string SenderName { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
