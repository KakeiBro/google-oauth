namespace GoogleOAuthPrototype.Application.POCO;

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresIn,
    string Scope);