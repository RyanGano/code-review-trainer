using Microsoft.AspNetCore.Authentication.JwtBearer;
using code_review_trainer_service.CodeReviewProblems;
using Microsoft.IdentityModel.Tokens;
using code_review_trainer_service.Services;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using OpenAI.Chat;
using Microsoft.Extensions.Options;
using System.Text.Json;

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


// Authentication (cleaned & hardened)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://login.microsoftonline.com/common/v2.0"; // multi-tenant v2 endpoint
        var configuredAudience = builder.Configuration["AzureAd:Audience"];
        if (string.IsNullOrWhiteSpace(configuredAudience))
        {
            throw new InvalidOperationException("AzureAd:Audience not configured.");
        }
        // Accept the bare GUID if config uses api://{guid}
        var canonicalAudience = configuredAudience.StartsWith("api://", StringComparison.OrdinalIgnoreCase)
            ? configuredAudience.Substring("api://".Length)
            : configuredAudience;
        options.Audience = canonicalAudience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // still multi-tenant; custom issuer filtering can be added later
            ValidateAudience = true,
            ValidAudience = canonicalAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine($"[Auth] Failed: {ctx.Exception.GetType().Name} - {ctx.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });

// Default authorization policy requiring access_as_user scope
builder.Services.AddAuthorization(o =>
{
    o.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireAssertion(ctx =>
            ctx.User.HasClaim(c => c.Type == "scp" && c.Value.Split(' ').Contains("access_as_user")) ||
            ctx.User.HasClaim(c => c.Type == "http://schemas.microsoft.com/identity/claims/scope" && c.Value.Split(' ').Contains("access_as_user")))
        .Build();
});
builder.Services.AddCodeReviewServices(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = new List<string>();

        allowedOrigins.Add("https://zealous-ocean-029a8df1e.2.azurestaticapps.net");

        if (builder.Environment.IsDevelopment())
        {
            allowedOrigins.Add("http://localhost:5173");
            allowedOrigins.Add("http://localhost:3000");
            allowedOrigins.Add("https://localhost:5173");
        }

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

app.MapGet("/tests/", (DifficultyLevel? level, Language language) =>
{
    if (level == null)
    {
        return Results.Ok(Enum.GetNames<DifficultyLevel>());
    }

    // Select a random problem from the appropriate list. Each language-specific
    // problem list may contain 'patch' style entries where Original is non-empty.
    CodeReviewProblem? randomProblem = level.Value switch
    {
        DifficultyLevel.Easy => language switch
        {
            Language.JavaScript => EasyJavaScriptCodeReviewProblems.GetRandomProblemWithId(),
            Language.TypeScript => EasyTypeScriptCodeReviewProblems.GetRandomProblemWithId(),
            _ => EasyCodeReviewProblems.GetRandomProblemWithId()
        },
        DifficultyLevel.Medium => language switch
        {
            Language.JavaScript => MediumJavaScriptCodeReviewProblems.GetRandomProblemWithId(),
            Language.TypeScript => MediumTypeScriptCodeReviewProblems.GetRandomProblemWithId(),
            _ => MediumCodeReviewProblems.GetRandomProblemWithId()
        },
        _ => null
    };

    if (randomProblem == null)
    {
        return Results.BadRequest(new { error = "Unsupported difficulty level" });
    }

    return Results.Ok(new
    {
        level = level.ToString(),
        language = language.ToString(),
        problem = new { original = randomProblem.Original, patched = randomProblem.Problem },
        id = randomProblem.Id,
        purpose = randomProblem.Purpose
    });
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

// Explain an individual item from a review result. For now this returns a static placeholder
// In future this can delegate to the AI/model to generate a contextual explanation.
// Explain an individual item from a review result. Accepts a JSON body with the item's full text
// (title, category/severity, and explanation). Example field name: "itemText".
app.MapPost("/tests/{id}/explain", async (string id, ExplainRequest body, IProblemRepository repo, ChatClient? chat, IOptions<AzureOpenAISettings> options) =>
{
    var problem = repo.Get(id);
    if (problem == null)
    {
        return Results.NotFound(new { error = "Problem not found" });
    }

    // If ChatClient or configuration missing, return fallback placeholder
    var aiSettings = options?.Value;
    if (chat == null || aiSettings == null || !aiSettings.IsConfigured)
    {
        var fallback = "Explanation goes here";
        return Results.Ok(new { explanation = fallback });
    }

    // Build prompt using original code and item text. Include the full original code.
    string code = problem.Value.Code ?? string.Empty;

    var system = new SystemChatMessage("You are a helpful, patient senior engineer. Return ONLY valid JSON (no markdown, no backticks, no commentary) using the schema: { \"explanation\": string, \"examples\": string (optional) }.");

    var userBuilder = $@"You recently reviewed this code and gave this feedback:
{body.ItemText}

Here is the original code:
```csharp
{code}
```

Please do the following:
1) Explain your feedback clearly and simply so the developer can fully understand the implications.
2) If the issue relates to bounds checking, provide example input values that produce bad output and show the bad output.
3) If the issue is a code-style or formatting issue, provide concrete guidance or examples in the examples field.
4) Re-evaluate your original review item and note any uncertainty or mistakes.

Return ONLY a single JSON object matching the schema: {{ ""explanation"": string, ""examples"": string }}. Do NOT include any markdown or extra text.";

    var messages = new List<ChatMessage>
    {
        system,
        new UserChatMessage(userBuilder)
    };

    try
    {
        var resp = await chat.CompleteChatAsync(messages, new ChatCompletionOptions { MaxOutputTokenCount = 1200, Temperature = 0.2f });
        var text = resp.Value?.Content?.FirstOrDefault()?.Text ?? string.Empty;

        // Try to extract a JSON object from the model's response
        int first = text.IndexOf('{');
        int last = text.LastIndexOf('}');
        if (first >= 0 && last > first)
        {
            var candidate = text.Substring(first, last - first + 1).Trim();
            try
            {
                var doc = JsonDocument.Parse(candidate);
                var root = doc.RootElement;
                var explanationStr = root.TryGetProperty("explanation", out var exEl) ? exEl.GetString() ?? string.Empty : string.Empty;
                var examplesStr = root.TryGetProperty("examples", out var exsEl) ? exsEl.GetString() ?? string.Empty : string.Empty;
                return Results.Ok(new { explanation = explanationStr, examples = examplesStr });
            }
            catch (Exception parseEx)
            {
                Console.WriteLine($"Failed to parse JSON from model: {parseEx.Message}");
                // fall through to return raw text explanation
            }
        }

        // If parsing failed, return the raw text as explanation in the structured form
        return Results.Ok(new { explanation = text, examples = string.Empty });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Explain call failed: {ex.Message}");
        return Results.Ok(new { explanation = "Explanation goes here", examples = string.Empty });
    }
})
.WithName("ExplainItem")
.RequireAuthorization();


app.Run();

public record ReviewSubmission(string review);
public record ExplainRequest(string ItemText);
