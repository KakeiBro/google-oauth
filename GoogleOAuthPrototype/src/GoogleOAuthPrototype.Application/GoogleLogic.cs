using System.Text.Json;

namespace GoogleOAuthPrototype.Application;

public static class GoogleLogic
{
    private static readonly JsonSerializerOptions SERIALIZER_OPTIONS = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private const string CLIENT_ID = "1066721632069-0s06qo68iq2e9hqupqjih8rnn1buad90.apps.googleusercontent.com";
    private const string CLIENT_SECRET = "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";
    private const string REDIRECT_URI = "http://localhost:5080/api/google-auth-callback";
    private const string AUTHORIZATION_ENDPOINT = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TOKEN_ENDPOINT = "https://oauth2.googleapis.com/token";
    private const string DEFAULT_ACCESS_TYPE = "online";
    private const string DEFAULT_PROMPT_VALUE = "consent";
    private const string EMPTY_STRING = "";

    public static IResult GenerateGoogleUrl(
        string accessType = DEFAULT_ACCESS_TYPE, string prompt = DEFAULT_PROMPT_VALUE)
    {
        var authorizationUrl = $"{AUTHORIZATION_ENDPOINT}?client_id={CLIENT_ID}" +
                               $"&redirect_uri={Uri.EscapeDataString(REDIRECT_URI)}" +
                               $"&response_type=code&scope=openid email profile" +
                               $"&access_type={accessType}" +
                               $"&prompt={prompt}";
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

    public static async Task<IResult> GoogleTokensAsync(string code = EMPTY_STRING)
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