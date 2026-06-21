namespace AgentForge.Orchestrator.Exceptions
{
    /// <summary>
    /// Thrown when registration is attempted with an email that already exists.
    /// Maps to HTTP 409 Conflict.
    /// </summary>
    public class EmailAlreadyExistsException : Exception
    {
        public EmailAlreadyExistsException(string message) : base(message)
        {
        }
    }
}
