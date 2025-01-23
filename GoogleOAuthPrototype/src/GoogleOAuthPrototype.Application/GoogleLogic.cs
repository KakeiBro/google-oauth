using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace GoogleOAuthPrototype.Application;

public static class GoogleLogic
{
    private static readonly JsonSerializerOptions SERIALIZER_OPTIONS = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private const string CLIENT_ID = "CLIENT_ID";
    private const string CLIENT_SECRET = "CLIENT_SECRET";
    private const string REDIRECT_URI = "http://localhost:5080/api/google-auth-callback";
    private const string AUTHORIZATION_ENDPOINT = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TOKEN_ENDPOINT = "https://oauth2.googleapis.com/token";

    public static IResult GenerateGoogleUrl()
    {
        var authorizationUrl = $"{AUTHORIZATION_ENDPOINT}?client_id={CLIENT_ID}" +
                               $"&redirect_uri={Uri.EscapeDataString(REDIRECT_URI)}" +
                               $"&response_type=code&scope=openid email profile";
        return Results.Ok(new
        {
            Message = $"Visit the following URL to authorize the app: {authorizationUrl}"
        });
    }

    public static IResult GoogleAuthCallback(HttpRequest request)
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

    public static async Task<IResult> GoogleTokensAsync(HttpRequest request)
    {
        using var httpClient = new HttpClient();

        if (!request.Query.TryGetValue("code", out var authCode)
            || authCode.Count == 0)
        {
            return Results.BadRequest(new
            {
                Message = "Please provide a valid code"
            });
        }

        var requestContent = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("code", authCode[0]!),
            new KeyValuePair<string, string>("client_id", CLIENT_ID),
            new KeyValuePair<string, string>("client_secret", CLIENT_SECRET),
            new KeyValuePair<string, string>("redirect_uri", REDIRECT_URI),
            new KeyValuePair<string, string>("grant_type", "authorization_code")
        ]);

        var response = await httpClient.PostAsync(TOKEN_ENDPOINT, requestContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error fetching tokens: {response.StatusCode} {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        return Results.Ok(new
        {
            Response = JsonSerializer.Deserialize<TokenResponse>(responseContent, SERIALIZER_OPTIONS),
            RawResponse = responseContent
        });
    }

    // Helper class to parse token response
    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string Scope { get; set; }
    }
}