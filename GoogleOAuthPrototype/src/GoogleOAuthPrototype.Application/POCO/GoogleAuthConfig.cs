using System.Text.Json.Serialization;

namespace GoogleOAuthPrototype.Application.POCO;

public class GoogleAuthConfig
{
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
}