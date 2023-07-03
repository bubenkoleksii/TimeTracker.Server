namespace TimeTracker.Server.Shared.Exceptions;

public class AuthenticationException : Exception
{
    public AuthenticationException()
    {
    }

    public AuthenticationException(string message) : base($"Authentication failed: {message}")
    {
    }
}