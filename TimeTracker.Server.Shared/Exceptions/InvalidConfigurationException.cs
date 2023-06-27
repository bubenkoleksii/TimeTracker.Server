namespace TimeTracker.Server.Shared.Exceptions;

public class InvalidConfigurationException : Exception
{
    public InvalidConfigurationException()
    {
    }

    public InvalidConfigurationException(string message) : base($"Invalid configuration: {message}")
    {
    }
}