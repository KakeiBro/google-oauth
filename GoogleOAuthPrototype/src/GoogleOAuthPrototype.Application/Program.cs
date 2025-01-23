var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("health", () => Results.Ok(new { Hello = "World" }));
app.MapGet("api/generate-url", GoogleLogic.GenerateGoogleUrl);
app.MapGet("api/google-auth-callback", GoogleLogic.GoogleAuthCallback);
app.MapGet("api/get-tokens", GoogleLogic.GoogleTokensAsync);

app.Run();