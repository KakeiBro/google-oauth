using System.Text.Json;

namespace GoogleOAuthPrototype.Application.Services;

public class GoogleService(IOptions<GoogleAuthConfig> configuration) : IGoogleService
{
    public IResult GenerateGoogleUrl(string accessType, string prompt)
    {
        var authorizationUrl = $"{GoogleSettings.AUTHORIZATION_ENDPOINT}?client_id={configuration.Value.ClientId}" +
                               $"&redirect_uri={Uri.EscapeDataString(GoogleSettings.REDIRECT_URI)}" +
                               $"&response_type=code&scope=openid email profile" +
                               $"&access_type={accessType}" +
                               $"&prompt={prompt}";
        return Results.Ok(new
        {
            Message = $"Visit the following URL to authorize the app: {authorizationUrl}"
        });
    }

    public IResult GoogleAuthCallback(HttpRequest request)
    {
        var queryParams = request.Query
            .Select(x => $"{x.Key}={x.Value}");

        return Results.Ok(new
        {
            // Back = JsonSerializer.Serialize(request.Body),
            QueryString = request.QueryString.ToString(),
            ParamValues = queryParams,
        });
    }

    public async Task<IResult> GoogleTokensAsync(string code)
    {
        using var httpClient = new HttpClient();

        if (string.IsNullOrWhiteSpace(code))
        {
            return Results.BadRequest(new
            {
                Message = "Please provide a valid code"
            });
        }

        var requestContent = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("client_id", configuration.Value.ClientId),
            new KeyValuePair<string, string>("client_secret", configuration.Value.ClientSecret),
            new KeyValuePair<string, string>("redirect_uri", GoogleSettings.REDIRECT_URI),
            new KeyValuePair<string, string>("grant_type", "authorization_code")
        ]);

        var response = await httpClient.PostAsync(GoogleSettings.TOKEN_ENDPOINT, requestContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error fetching tokens: {response.StatusCode} {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        return Results.Ok(new
        {
            Response = JsonSerializer.Deserialize<TokenResponse>(
                responseContent, AppSerializationOptions.GoogleConventionOptions),
            RawResponse = responseContent
        });
    }
}