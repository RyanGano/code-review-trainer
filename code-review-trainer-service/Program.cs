using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}";
        // Use the API scope audience, not the client ID
        options.Audience = builder.Configuration["AzureAd:Audience"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // Add some tolerance for clock skew
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

builder.Services.AddAuthorization();

// Add CORS - support both development and production origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = new List<string>();

        // Production origins
        allowedOrigins.Add("https://zealous-ocean-029a8df1e.2.azurestaticapps.net");

        // Development origins
        if (builder.Environment.IsDevelopment())
        {
            allowedOrigins.Add("http://localhost:5173");
            allowedOrigins.Add("http://localhost:3000");
            allowedOrigins.Add("https://localhost:5173");
        }

        // Add configured origins from appsettings
        var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (configuredOrigins != null)
        {
            allowedOrigins.AddRange(configuredOrigins);
        }

        policy.WithOrigins(allowedOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapGet("/", () => "I'm ALIVE!");

app.MapGet("/user", (HttpContext context) =>
{
    var user = context.User;
    return new
    {
        Name = user.Identity?.Name,
        IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
        Claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList()
    };
}).RequireAuthorization();

app.MapGet("/tests/", (DifficultyLevel? level) =>
{
    // If no level is provided, return all available levels
    if (level == null)
    {
        return Results.Ok(Enum.GetNames<DifficultyLevel>());
    }

    return Results.Ok($"Test level: {level}");
})
.WithName("GetTests")
.RequireAuthorization();

app.Run();

// Enum for difficulty levels
public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard,
    ExtraHard
}
