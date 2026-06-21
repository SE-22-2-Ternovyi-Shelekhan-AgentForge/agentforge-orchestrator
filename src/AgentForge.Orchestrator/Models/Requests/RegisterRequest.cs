using System.ComponentModel.DataAnnotations;

namespace AgentForge.Orchestrator.Models
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [MaxLength(255)]
        public string DisplayName { get; set; }
    }
}
