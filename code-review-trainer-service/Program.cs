using Microsoft.AspNetCore.Authentication.JwtBearer;
using code_review_trainer_service.CodeReviewProblems;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add authentication
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
    });

builder.Services.AddAuthorization();

// Add CORS
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
    if (level == null)
    {
        return Results.Ok(Enum.GetNames<DifficultyLevel>());
    }

    // Return a random problem for Easy level
    if (level == DifficultyLevel.Easy)
    {
        var randomProblem = EasyCodeReviewProblems.GetRandomProblemWithId();
        return Results.Ok(new { 
            level = level.ToString(), 
            problem = randomProblem.Problem,
            id = randomProblem.Id 
        });
    }

    // Return a random problem for Medium level
    if (level == DifficultyLevel.Medium)
    {
        var randomProblem = MediumCodeReviewProblems.GetRandomProblemWithId();
        return Results.Ok(new { 
            level = level.ToString(), 
            problem = randomProblem.Problem,
            id = randomProblem.Id 
        });
    }

    return Results.Ok($"Test level: {level}");
})
.WithName("GetTests")
.RequireAuthorization();

app.Run();
