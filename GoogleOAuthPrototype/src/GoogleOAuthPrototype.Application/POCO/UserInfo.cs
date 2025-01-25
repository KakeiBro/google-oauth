namespace GoogleOAuthPrototype.Application.POCO;

public record UserInfo(
    string Id,
    string Email,
    bool VerifiedEmail,
    string Name,
    string GivenName,
    string FamilyName,
    string Picture);