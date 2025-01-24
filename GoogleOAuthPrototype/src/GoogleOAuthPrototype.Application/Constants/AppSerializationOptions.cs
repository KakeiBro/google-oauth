using System.Text.Json;

namespace GoogleOAuthPrototype.Application.Constants;

public static class AppSerializationOptions
{
    public static readonly JsonSerializerOptions GoogleConventionOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
}