namespace AgentForge.Orchestrator.Exceptions
{
    /// <summary>
    /// Thrown when an authenticated user attempts to access or modify a resource
    /// that belongs to another user. Maps to HTTP 403 Forbidden.
    /// </summary>
    public class ForbiddenAccessException : Exception
    {
        public ForbiddenAccessException(string message) : base(message)
        {
        }
    }
}
