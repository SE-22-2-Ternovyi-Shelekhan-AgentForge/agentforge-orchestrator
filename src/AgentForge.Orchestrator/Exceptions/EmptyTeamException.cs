namespace AgentForge.Orchestrator.Exceptions
{
    /// <summary>
    /// Thrown when a conversation has a designated team, but that team has no
    /// agents configured. Maps to HTTP 422 Unprocessable Entity: the request is
    /// well-formed, but the referenced team cannot process a message.
    /// </summary>
    public class EmptyTeamException : Exception
    {
        public EmptyTeamException(string message) : base(message)
        {
        }
    }
}
