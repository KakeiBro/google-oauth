namespace GoogleOAuthPrototype.Application.Constants;

public static class GoogleSettings
{
    public const string REDIRECT_URI = "http://localhost:5080/api/google-auth-callback";
    public const string AUTHORIZATION_ENDPOINT = "https://accounts.google.com/o/oauth2/v2/auth";
    public const string TOKEN_ENDPOINT = "https://oauth2.googleapis.com/token";
    public const string DEFAULT_ACCESS_TYPE = "online";
    public const string DEFAULT_PROMPT_VALUE = "consent";
    public const string EMPTY_STRING = "";
}