namespace TimeTracker.Server.Shared.Exceptions;

public class AuthorizationException : Exception
{
    public AuthorizationException()
    {
    }

    public AuthorizationException(string message) : base($"Authorization failed: {message}")
    {
    }
}