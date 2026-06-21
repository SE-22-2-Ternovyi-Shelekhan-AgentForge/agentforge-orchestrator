namespace AgentForge.Orchestrator.Exceptions
{
    /// <summary>
    /// Thrown when login fails because the email is unknown or the password
    /// does not match. Maps to HTTP 401 Unauthorized.
    /// </summary>
    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException(string message) : base(message)
        {
        }
    }
}
