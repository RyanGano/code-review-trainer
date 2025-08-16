using Microsoft.AspNetCore.Authentication.JwtBearer;
using code_review_trainer_service.CodeReviewProblems;
using Microsoft.IdentityModel.Tokens;
using code_review_trainer_service.Services;
using Azure.Identity; // added for Key Vault
using Azure.Extensions.AspNetCore.Configuration.Secrets;

var builder = WebApplication.CreateBuilder(args);

// Attempt to add Azure Key Vault (production) before services build so configuration binds include secrets.
// Fallback: if environment variables AZURE_KEY_VAULT_NAME or AzureOpenAI__ApiKey are absent / access fails, continue with existing config (user-secrets/local).
var keyVaultName = Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_NAME");
if (!string.IsNullOrWhiteSpace(keyVaultName))
{
    try
    {
        var vaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
        var cred = new DefaultAzureCredential();
        builder.Configuration.AddAzureKeyVault(vaultUri, cred, new AzureKeyVaultConfigurationOptions
        {
            ReloadInterval = TimeSpan.FromMinutes(10)
        });
    }
    catch (Exception ex)
    {
        // Non-fatal: log to console and proceed (app may still rely on user-secrets/local settings)
        Console.WriteLine($"Key Vault integration skipped: {ex.Message}");
    }
}


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
builder.Services.AddCodeReviewServices(builder.Configuration);

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
        return Results.Ok(new
        {
            level = level.ToString(),
            problem = randomProblem.Problem,
            id = randomProblem.Id
        });
    }

    // Return a random problem for Medium level
    if (level == DifficultyLevel.Medium)
    {
        var randomProblem = MediumCodeReviewProblems.GetRandomProblemWithId();
        return Results.Ok(new
        {
            level = level.ToString(),
            problem = randomProblem.Problem,
            id = randomProblem.Id
        });
    }

    return Results.Ok($"Test level: {level}");
})
.WithName("GetTests")
.RequireAuthorization();

app.MapPost("/tests/{id}", async (string id, ReviewSubmission submission, IProblemRepository repo, ICodeReviewModel model) =>
{
    var problem = repo.Get(id);
    if (problem == null)
    {
        return Results.NotFound(new { error = "Problem not found" });
    }
    var result = await model.ReviewAsync(new CodeReviewRequest(problem.Value.Id, problem.Value.Code, submission.review));
    return Results.Ok(result);
})
.WithName("SubmitReview")
.RequireAuthorization();

app.Run();

// Record for review submission payload
public record ReviewSubmission(string review);
