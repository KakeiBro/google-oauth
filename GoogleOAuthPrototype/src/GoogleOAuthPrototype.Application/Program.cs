var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configurations
builder.Services.ConfigureGoogleAuthConfig(builder.Configuration);

// Services
builder.Services.AddSingleton<IGoogleService, GoogleService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("health", () => Results.Ok(new { Hello = "World" }));
app.MapGet("api/generate-url",
    (string accessType, string prompt, IGoogleService service) => service.GenerateGoogleUrl(accessType, prompt));
app.MapGet("api/google-auth-callback",
    (HttpRequest request, IGoogleService service) => service.GoogleAuthCallback(request));
app.MapGet("api/get-tokens", async (string code, IGoogleService service) => await service.GoogleTokensAsync(code));
app.MapGet("api/get-user-data",
    async (HttpRequest request, IGoogleService service) => await service.GetUserDataAsync(request));
app.MapGet("api/revoke-token",
    async (IGoogleService service, string? token = null) => await service.RevokeTokenAsync(token));
app.MapGet("api/refresh-token",
    async (IGoogleService service, string? refreshToken = null) => await service.RefreshTokenAsync(refreshToken));

app.Run();