using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var logger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("Startup");

// Log configuration values at startup
logger.LogInformation("=== Azure AD Configuration ===");
logger.LogInformation("TenantId: {TenantId}", builder.Configuration["AzureAd:TenantId"]);
logger.LogInformation("Audience: {Audience}", builder.Configuration["AzureAd:Audience"]);
logger.LogInformation("Authority: {Authority}", $"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}");
logger.LogInformation("Environment: {Environment}", builder.Environment.EnvironmentName);
logger.LogInformation("===============================");

// Add authentication with detailed logging
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}";
        options.Audience = builder.Configuration["AzureAd:Audience"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        // Add comprehensive event handlers for debugging
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                logger.LogInformation("JWT Token received. Length: {TokenLength}", token?.Length ?? 0);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                logger.LogInformation("✅ Token validated successfully for user: {UserName}",
                    context.Principal?.Identity?.Name ?? "Unknown");
                logger.LogInformation("Claims count: {ClaimsCount}", context.Principal?.Claims.Count() ?? 0);
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                logger.LogError("❌ Authentication failed: {Exception}", context.Exception?.Message);
                logger.LogError("Exception details: {ExceptionDetails}", context.Exception?.ToString());
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                logger.LogWarning("🔐 Authentication challenge triggered");
                logger.LogWarning("Error: {Error}", context.Error);
                logger.LogWarning("Error Description: {ErrorDescription}", context.ErrorDescription);
                logger.LogWarning("Error URI: {ErrorUri}", context.ErrorUri);
                return Task.CompletedTask;
            }
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

        logger.LogInformation("CORS allowed origins: {Origins}", string.Join(", ", allowedOrigins));

        policy.WithOrigins(allowedOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Log middleware pipeline
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("🌐 Request: {Method} {Path} from {Origin}",
        context.Request.Method,
        context.Request.Path,
        context.Request.Headers["Origin"].FirstOrDefault() ?? "No Origin");

    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    logger.LogInformation("Auth header present: {HasAuth}, Length: {Length}",
        !string.IsNullOrEmpty(authHeader),
        authHeader?.Length ?? 0);

    await next();

    logger.LogInformation("Response: {StatusCode}", context.Response.StatusCode);
});

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// Add debug endpoints
app.MapGet("/debug/config", () =>
{
    var config = builder.Configuration;
    return new
    {
        TenantId = config["AzureAd:TenantId"],
        Audience = config["AzureAd:Audience"],
        Authority = $"https://login.microsoftonline.com/{config["AzureAd:TenantId"]}",
        Environment = builder.Environment.EnvironmentName,
        AllConfigKeys = config.AsEnumerable().Where(c => c.Key.StartsWith("AzureAd")).ToDictionary(c => c.Key, c => c.Value)
    };
});

app.MapGet("/debug/headers", (HttpContext context) =>
{
    var headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

    return new
    {
        Headers = headers,
        HasAuthHeader = !string.IsNullOrEmpty(authHeader),
        AuthHeaderLength = authHeader?.Length ?? 0,
        IsHttps = context.Request.IsHttps,
        Scheme = context.Request.Scheme,
        Host = context.Request.Host.ToString(),
        UserAuthenticated = context.User.Identity?.IsAuthenticated ?? false
    };
});

// Endpoints
app.MapGet("/", () => "I'm ALIVE!");

app.MapGet("/user", (HttpContext context) =>
{
    var user = context.User;
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("👤 /user endpoint called");
    logger.LogInformation("User authenticated: {IsAuthenticated}", user.Identity?.IsAuthenticated);
    logger.LogInformation("User name: {UserName}", user.Identity?.Name);
    logger.LogInformation("Claims count: {ClaimsCount}", user.Claims.Count());

    foreach (var claim in user.Claims.Take(5)) // Log first 5 claims
    {
        logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
    }

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
