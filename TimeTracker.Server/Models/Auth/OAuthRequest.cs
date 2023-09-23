namespace TimeTracker.Server.Models.Auth;

public class OAuthRequest
{
    public string ClientId { get; set; } = string.Empty;

    public string Credential { get; set; } = string.Empty;

    public string SelectBy { get; set; } = string.Empty;
}