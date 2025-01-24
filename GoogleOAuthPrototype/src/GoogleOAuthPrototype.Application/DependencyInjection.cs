using System.Text.Json;

namespace GoogleOAuthPrototype.Application;

public static class DependencyInjection
{
    private const string SecretsSectionName = "web";
    private const string SecretsFileName = "client_secrets.json";

    public static IServiceCollection ConfigureGoogleAuthConfig(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Files", SecretsFileName);
        var file = File.ReadAllText(path);
        
        using var document = JsonDocument.Parse(file);
        
        if (!document.RootElement.TryGetProperty(SecretsSectionName, out var webElement))
        {
            throw new Exception("Invalid secrets file.");
        }

        // Deserialize the specific section into the target object
        var secrets = JsonSerializer.Deserialize<GoogleAuthConfig>(
            webElement.GetRawText(), AppSerializationOptions.GoogleConventionOptions);

        // Bind the configuration section to the strongly-typed class
        services.Configure<GoogleAuthConfig>(options =>
        {
            options.ClientId = secrets!.ClientId;
            options.ClientSecret = secrets.ClientSecret;
        });

        return services;
    }
}