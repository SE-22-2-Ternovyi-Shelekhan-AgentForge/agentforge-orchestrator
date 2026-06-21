namespace AgentForge.Orchestrator.Models
{
    /// <summary>
    /// Returned immediately after a user message is accepted. Contains the
    /// persisted user message and the identifier of the agent session that was
    /// dispatched, so the client can correlate the eventual trace / results.
    /// </summary>
    public class SendMessageResponse
    {
        public ChatMessageDto Message { get; set; }
        public Guid SessionId { get; set; }
    }
}
