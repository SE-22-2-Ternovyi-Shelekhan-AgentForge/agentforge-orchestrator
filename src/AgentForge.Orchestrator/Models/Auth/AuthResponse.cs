namespace AgentForge.Orchestrator.Models
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
    }
}
