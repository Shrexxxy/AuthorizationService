namespace IdentityService.Api.Options;

public class IdentitySettings
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public List<string> Scopes { get; set; }
}