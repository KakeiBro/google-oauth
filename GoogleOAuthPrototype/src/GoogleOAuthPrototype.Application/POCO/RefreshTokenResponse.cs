namespace GoogleOAuthPrototype.Application.POCO;

public record RefreshTokenResponse(string AccessToken, int ExpiresIn, string Scope, string TokenType);