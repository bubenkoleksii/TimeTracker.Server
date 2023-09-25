namespace TimeTracker.Server.Business.Models.Auth;

public class OAuthBusinessRequest
{
    public string ClientId { get; set; } = string.Empty;

    public string Credential { get; set; } = string.Empty;

    public string SelectBy { get; set; } = string.Empty;
}