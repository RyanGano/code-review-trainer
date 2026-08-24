using Microsoft.AspNetCore.Authentication.JwtBearer;
using code_review_trainer_service.CodeReviewProblems;
using Microsoft.IdentityModel.Tokens;
using code_review_trainer_service.Services;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.AI.OpenAI.Chat;
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
            ? configuredAudience["api://".Length..]
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

// Serialize enums (notably ReviewStatus) as their names so clients see "Approve"/"Reject".
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        List<string> allowedOrigins = [];

        allowedOrigins.Add("https://zealous-ocean-029a8df1e.2.azurestaticapps.net");

        if (builder.Environment.IsDevelopment())
        {
            allowedOrigins.Add("http://localhost:5173");
            allowedOrigins.Add("http://localhost:3000");
            allowedOrigins.Add("https://localhost:5173");
        }

        var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (configuredOrigins is not null)
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

app.MapMethods("/", ["GET", "HEAD"], () => "I'm ALIVE!");

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
    if (level is null)
    {
        return Results.Ok(Enum.GetNames<DifficultyLevel>());
    }

    // Resolve providers from DI and pick the matching provider for language+difficulty
    var providers = app.Services.GetServices<IProblemProvider>();
    var provider = providers.FirstOrDefault(p => p.Language == language && p.Difficulty == level.Value);
    if (provider is null)
    {
        return Results.BadRequest(new { error = "Unsupported difficulty level or language" });
    }

    var randomProblem = provider.GetRandomProblemWithId();
    if (randomProblem is null)
    {
        return Results.BadRequest(new { error = "No problems available for selected provider" });
    }

    return Results.Ok(new
    {
        level = level.ToString(),
        language = randomProblem.Language.ToString(),
        patch = randomProblem.Patch,
        id = randomProblem.Id,
        purpose = randomProblem.Purpose
    });
})
.WithName("GetTests")
.RequireAuthorization();

app.MapPost("/tests/{id}", async (string id, ReviewSubmission submission, IProblemRepository repo, ICodeReviewModel model) =>
{
    var problem = repo.Get(id);
    if (problem is null)
    {
        return Results.NotFound(new { error = "Problem not found" });
    }
    var result = await model.ReviewAsync(new CodeReviewRequest(
        problem.Id,
        problem.Code,
        submission.review,
        problem.Purpose,
        problem.Review,
        submission.isShippableAsIs));
    return Results.Ok(result);
})
.WithName("SubmitReview")
.RequireAuthorization();

// Explain one issue from a review result in more depth. The issue is identified by its stored id,
// so the model is given the reference review's own wording, category and severity rather than
// re-deriving them from a string the client assembled.
app.MapPost("/tests/{id}/explain", async (string id, ExplainRequest body, IProblemRepository repo, ChatClient? chat, IOptions<AzureOpenAISettings> options) =>
{
    var problem = repo.Get(id);
    if (problem is null)
    {
        return Results.NotFound(new { error = "Problem not found" });
    }

    // Prefer the stored issue; fall back to the client-supplied text for older clients.
    var issue = string.IsNullOrWhiteSpace(body.IssueId)
        ? null
        : problem.Review.Issues.FirstOrDefault(i => string.Equals(i.Id, body.IssueId, StringComparison.OrdinalIgnoreCase));

    if (issue is null && string.IsNullOrWhiteSpace(body.ItemText))
    {
        return Results.BadRequest(new { error = "Provide either issueId or itemText" });
    }

    var aiSettings = options?.Value;
    if (chat is null || aiSettings is null || !aiSettings.IsConfigured)
    {
        // Without a model we can still return the reference explanation we already hold.
        return Results.Ok(new { explanation = issue?.Explanation ?? "Explanation goes here", examples = string.Empty });
    }

    var codeFenceLanguage = problem.Language switch
    {
        Language.CSharp => "csharp",
        Language.JavaScript => "javascript",
        Language.TypeScript => "typescript",
        _ => "csharp"
    };

    var issueContext = issue is not null
        ? $"""
          Title: {issue.Title}
          Category: {issue.Category}
          Severity: {issue.Severity}
          Why it matters: {issue.Explanation}
          """
        : body.ItemText;

    var system = new SystemChatMessage(
        "You are a patient senior engineer explaining a code review finding to the developer who wrote the patch. " +
        "The finding is established and correct - your job is to make it land, not to re-litigate whether it is real. " +
        "Write to the developer in second person, plainly, without restating the finding verbatim.");

    var userBuilder = $@"A code review of this patch raised the following issue:

{issueContext}

Here is the patch it was raised against:
```{codeFenceLanguage}
{problem.Code}
```

The patch was intended to: {problem.Purpose}

In ""explanation"", explain what actually goes wrong and why it matters, concretely enough that the developer can find it in the code above and fix it.

In ""examples"", show it concretely when that helps: for a bounds or input-handling issue give specific input values and the wrong output or exception they produce; for a correctness issue walk through the case that breaks; for a style or structure issue show the before and after. Use a fenced {codeFenceLanguage} block for code. Leave it empty if a worked example would add nothing.";

    List<ChatMessage> messages = [system, new UserChatMessage(userBuilder)];

    try
    {
        var explainOptions = new ChatCompletionOptions
        {
            MaxOutputTokenCount = 4000,
            // Same reasoning as the grading call: a strict schema beats asking for JSON politely.
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                "code_review_explanation",
                BinaryData.FromString("""
                {
                  "type": "object",
                  "properties": {
                    "explanation": { "type": "string" },
                    "examples": { "type": "string" }
                  },
                  "required": ["explanation", "examples"],
                  "additionalProperties": false
                }
                """),
                jsonSchemaIsStrict: true)
        };
#pragma warning disable AOAI001
        explainOptions.SetNewMaxCompletionTokensPropertyEnabled(true);
#pragma warning restore AOAI001

        var resp = await chat.CompleteChatAsync(messages, explainOptions);
        var text = resp.Value?.Content?.FirstOrDefault()?.Text ?? string.Empty;

        int first = text.IndexOf('{');
        int last = text.LastIndexOf('}');
        if (first >= 0 && last > first)
        {
            try
            {
                var root = JsonDocument.Parse(text[first..(last + 1)]).RootElement;
                return Results.Ok(new
                {
                    explanation = root.TryGetProperty("explanation", out var exEl) ? exEl.GetString() ?? string.Empty : string.Empty,
                    examples = root.TryGetProperty("examples", out var exsEl) ? exsEl.GetString() ?? string.Empty : string.Empty
                });
            }
            catch (JsonException parseEx)
            {
                app.Logger.LogWarning(parseEx, "Explain: could not parse model JSON for issue {IssueId} of {ProblemId}", body.IssueId, id);
            }
        }

        // Structured outputs make this unreachable in practice; return the text rather than nothing.
        return Results.Ok(new { explanation = text, examples = string.Empty });
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Explain call failed for issue {IssueId} of {ProblemId}", body.IssueId, id);
        return Results.Ok(new { explanation = issue?.Explanation ?? "Explanation goes here", examples = string.Empty });
    }
})
.WithName("ExplainItem")
.RequireAuthorization();


app.Run();

public record ReviewSubmission(string review, bool? isShippableAsIs = null);
/// <summary>
/// Identifies the review item to explain. IssueId is preferred - the server then uses the stored
/// reference wording. ItemText is kept for clients that still assemble the text themselves.
/// </summary>
public record ExplainRequest(string? IssueId = null, string? ItemText = null);
