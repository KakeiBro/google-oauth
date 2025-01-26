namespace GoogleOAuthPrototype.Application.Services;

public interface IGoogleService
{
    IResult GenerateGoogleUrl(
        string accessType = GoogleSettings.DEFAULT_ACCESS_TYPE,
        string prompt = GoogleSettings.DEFAULT_PROMPT_VALUE);

    IResult GoogleAuthCallback(HttpRequest request);

    Task<IResult> GoogleTokensAsync(string code = GoogleSettings.EMPTY_STRING);
    
    Task<IResult> GetUserDataAsync(HttpRequest request);
    
    Task<IResult> RevokeTokenAsync(string? token);
}