using System.Net.Http.Headers;
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
        if (string.IsNullOrWhiteSpace(code))
        {
            return Results.BadRequest(new
            {
                Message = "Please provide a valid code"
            });
        }

        using var httpClient = new HttpClient();

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

    public async Task<IResult> GetUserDataAsync(HttpRequest request)
    {
        var bearerIsPresent = request.Headers.TryGetValue(
            "Authorization", out var bearerToken);
        if (!bearerIsPresent || string.IsNullOrWhiteSpace(bearerToken))
        {
            return Results.BadRequest(new
            {
                Message = "Invalid token"
            });
        }

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", bearerToken[0]);

        var response = await httpClient.GetAsync(GoogleSettings.USER_INFO_ENDPOINT);

        var profileResponse = JsonSerializer.Deserialize<UserInfo>(
            await response.Content.ReadAsStringAsync(),
            AppSerializationOptions.GoogleConventionOptions);

        return Results.Ok(new
        {
            ProfileResponse = profileResponse
        });
    }

    public async Task<IResult> RevokeTokenAsync(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Results.BadRequest(new
            {
                Message = "Please provide a valid token",
            });
        }
        
        using var httpClient = new HttpClient();
        
        var requestContent = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("token", token),
        ]);

        var response = await httpClient.PostAsync(
            GoogleSettings.REVOKE_TOKEN_ENDPOINT, requestContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error revoking token: {response.StatusCode} {errorContent}");
        }
        
        var responseContent = await response.Content.ReadAsStringAsync();
        return Results.Ok(new
        {
            Response = responseContent,
            Message = "Revoked"
        });
    }
}