using System.Text.Json;
using OpenAI.Chat;
using Microsoft.Extensions.Options;

namespace code_review_trainer_service.Services;

/// <summary>
/// Calls Azure OpenAI Chat Completions (via SDK) to perform a structured code review.
/// Required config keys: AzureOpenAI:Endpoint, AzureOpenAI:ApiKey (user-secrets), AzureOpenAI:DeploymentName
/// </summary>
public class AzureOpenAICodeReviewModel : ICodeReviewModel
{
  private readonly ChatClient _chat;
  private readonly ILogger<AzureOpenAICodeReviewModel> _logger;
  private readonly AzureOpenAISettings _options;

  public AzureOpenAICodeReviewModel(ChatClient chat, ILogger<AzureOpenAICodeReviewModel> logger, IOptions<AzureOpenAISettings> options)
  {
    _chat = chat;
    _logger = logger;
    _options = options.Value;
  }

  public async Task<CodeReviewModelResult> ReviewAsync(CodeReviewRequest request, CancellationToken ct = default)
  {
    if (!_options.IsConfigured)
    {
      _logger.LogWarning("Azure OpenAI not fully configured. Endpoint={Endpoint} DeploymentName={DeploymentName} ApiKeyPresent={ApiKeyPresent}", _options.Endpoint, _options.DeploymentName, string.IsNullOrEmpty(_options.ApiKey) ? "NO" : "YES");
      return Fallback(request.ProblemId, "Azure OpenAI not configured");
    }

    try
    {
      var systemPrompt = "You are a senior C# engineer. Output ONLY valid, minified JSON object per the schema. ABSOLUTELY NO markdown, no backticks, no commentary. If unsure, output an empty JSON object with required keys.";
      var userPrompt = BuildUserPrompt(request);

      var messages = new List<ChatMessage>
      {
        new SystemChatMessage(systemPrompt),
        new UserChatMessage(userPrompt)
      };

      var options = new ChatCompletionOptions
      {
        MaxOutputTokenCount = 800,
        Temperature = 0.2f,
        TopP = 1.0f
      };

      var response = await _chat.CompleteChatAsync(messages, options, ct);
      var content = response.Value?.Content?.FirstOrDefault()?.Text ?? string.Empty;
      var originalRaw = content;
      content = CleanModelContent(content);

      if (string.IsNullOrWhiteSpace(content))
      {
        return Fallback(request.ProblemId, "EmptyResponse", raw: content);
      }

      try
      {
        if (!content.TrimStart().StartsWith("{"))
        {
          var repaired = TryExtractJson(content);
          if (!string.IsNullOrWhiteSpace(repaired)) content = repaired!;
        }
        var modelObj = JsonDocument.Parse(content);
        return MapModelJson(request.ProblemId, content, modelObj.RootElement);
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "Model returned non-JSON; attempting repair");
        var repaired = TryRepairJson(content);
        if (repaired != null)
        {
          try
          {
            var repairedDoc = JsonDocument.Parse(repaired);
            return MapModelJson(request.ProblemId, repaired, repairedDoc.RootElement);
          }
          catch (Exception ex2)
          {
            _logger.LogWarning(ex2, "Repair attempt failed");
          }
        }
        return Fallback(request.ProblemId, "Non-JSON response", raw: originalRaw);
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Azure OpenAI chat call failed");
      return Fallback(request.ProblemId, "Exception", ex.Message);
    }
  }

  private CodeReviewModelResult MapModelJson(string problemId, string raw, JsonElement el)
  {
    double overall = el.TryGetProperty("overallScore", out var os) && os.TryGetDouble(out var d) ? d : 0;
    var issues = new List<CodeReviewIssue>();
    if (el.TryGetProperty("issuesDetected", out var issuesArr) && issuesArr.ValueKind == JsonValueKind.Array)
    {
      foreach (var i in issuesArr.EnumerateArray())
      {
        string issueId = string.Empty;
        if (i.TryGetProperty("id", out var idEl))
        {
          if (idEl.ValueKind == JsonValueKind.Number) issueId = idEl.GetRawText();
          else issueId = idEl.GetString() ?? string.Empty;
        }
        string category = i.TryGetProperty("category", out var cat) ? AsFlexibleString(cat) : string.Empty;
        string title = i.TryGetProperty("title", out var t) ? AsFlexibleString(t) : string.Empty;
        string explanation = i.TryGetProperty("explanation", out var ex) ? AsFlexibleString(ex) : string.Empty;
        string severity = i.TryGetProperty("severity", out var sev) ? AsFlexibleString(sev) : string.Empty;
        issues.Add(new CodeReviewIssue(issueId, category, title, explanation, severity));
      }
    }
    var matched = new List<CodeReviewMatchedUserPoint>();
    if (el.TryGetProperty("matchedUserPoints", out var mup) && mup.ValueKind == JsonValueKind.Array)
    {
      foreach (var m in mup.EnumerateArray())
      {
        string[] mids = Array.Empty<string>();
        if (m.TryGetProperty("matchedIssueIds", out var mi) && mi.ValueKind == JsonValueKind.Array)
        {
          mids = mi.EnumerateArray().Select(AsFlexibleString).ToArray();
        }
        string excerpt = m.TryGetProperty("excerpt", out var ex2) ? AsFlexibleString(ex2) : string.Empty;
        string accuracy = m.TryGetProperty("accuracy", out var acc) ? AsFlexibleString(acc) : string.Empty;
        matched.Add(new CodeReviewMatchedUserPoint(excerpt, mids, accuracy));
      }
    }
    var missed = new List<string>();
    if (el.TryGetProperty("missedCriticalIssueIds", out var mc) && mc.ValueKind == JsonValueKind.Array)
    {
      missed.AddRange(mc.EnumerateArray().Select(AsFlexibleString));
    }
    var summary = el.TryGetProperty("summary", out var sum) ? sum.GetString() ?? string.Empty : string.Empty;
    return new CodeReviewModelResult(problemId, overall, issues, matched, missed, summary, raw, false, null);
  }

  private static string BuildUserPrompt(CodeReviewRequest req)
  {
    var truncatedCode = Truncate(req.Code, 2500);
    var truncatedReview = Truncate(req.UserReview, 1500);
    // Escape braces by doubling for string interpolation
    var schema = "{{ problemId, overallScore, issuesDetected:[{{id,category,title,explanation,severity}}], matchedUserPoints:[{{excerpt,matchedIssueIds,accuracy}}], missedCriticalIssueIds:[], summary }}";
    return $"ProblemId: {req.ProblemId}\nOriginalCode:\n```csharp\n{truncatedCode}\n```\nUserReview:\n{truncatedReview}\nReturn ONLY RAW JSON (no markdown fences) matching schema: {schema}.";
  }

  private static string Truncate(string s, int max) => s.Length <= max ? s : s.Substring(0, max) + "\n/* truncated */";

  private CodeReviewModelResult Fallback(string problemId, string reason, string? details = null, string? raw = null) =>
      new(
          ProblemId: problemId,
          OverallScore: 0,
          IssuesDetected: Array.Empty<CodeReviewIssue>(),
          MatchedUserPoints: Array.Empty<CodeReviewMatchedUserPoint>(),
          MissedCriticalIssueIds: Array.Empty<string>(),
          Summary: $"Fallback: {reason} {(details ?? string.Empty)}",
          RawModelJson: raw ?? string.Empty,
          IsFallback: true,
          Error: reason + (details != null ? ": " + details : string.Empty)
      );

  // JSON cleaning + repair helpers below support lenient parsing of model output.

  private static string CleanModelContent(string content)
  {
    if (string.IsNullOrWhiteSpace(content)) return content;
    // Strip markdown code fences ```json ... ``` or ``` ... ```
    if (content.StartsWith("```"))
    {
      // Remove starting fence
      var firstNewline = content.IndexOf('\n');
      if (firstNewline > -1)
      {
        var header = content.Substring(0, firstNewline).Trim(); // e.g., ```json
        if (header.StartsWith("```"))
        {
          content = content.Substring(firstNewline + 1);
        }
      }
      // Remove trailing fence
      var fenceIndex = content.LastIndexOf("```", StringComparison.Ordinal);
      if (fenceIndex >= 0)
      {
        content = content.Substring(0, fenceIndex);
      }
    }
    // Trim and attempt to isolate JSON object
    content = content.Trim();
    var firstBrace = content.IndexOf('{');
    var lastBrace = content.LastIndexOf('}');
    if (firstBrace >= 0 && lastBrace > firstBrace)
    {
      content = content.Substring(firstBrace, lastBrace - firstBrace + 1);
    }
    return content.Trim();
  }

  private static string AsFlexibleString(JsonElement el)
  {
    return el.ValueKind switch
    {
      JsonValueKind.String => el.GetString() ?? string.Empty,
      JsonValueKind.Number => el.GetRawText(),
      JsonValueKind.True => "true",
      JsonValueKind.False => "false",
      JsonValueKind.Null => string.Empty,
      _ => el.GetRawText()
    };
  }

  private static string? TryExtractJson(string content)
  {
    if (string.IsNullOrWhiteSpace(content)) return null;
    int first = content.IndexOf('{');
    int last = content.LastIndexOf('}');
    if (first < 0 || last <= first) return null;
    var candidate = content.Substring(first, last - first + 1).Trim();
    return candidate;
  }

  private static string? TryRepairJson(string content)
  {
    var candidate = TryExtractJson(content);
    if (candidate == null) return null;
    // Remove trailing ellipsis if present before final brace
    candidate = candidate.Replace("...\n", "").Replace("...", "");
    if (IsBracesBalanced(candidate)) return candidate;
    // Try trimming until balanced or too small
    for (int i = candidate.Length - 1; i > 0; i--)
    {
      if (candidate[i] == '}' || candidate[i] == ']')
      {
        var sub = candidate.Substring(0, i + 1);
        if (IsBracesBalanced(sub)) return sub;
      }
    }
    return null;
  }

  private static bool IsBracesBalanced(string json)
  {
    int brace = 0; int bracket = 0; bool inStr = false; bool esc = false;
    foreach (var c in json)
    {
      if (esc) { esc = false; continue; }
      if (c == '\\') { esc = true; continue; }
      if (c == '"') { inStr = !inStr; continue; }
      if (inStr) continue;
      if (c == '{') brace++; else if (c == '}') brace--; else if (c == '[') bracket++; else if (c == ']') bracket--;
      if (brace < 0 || bracket < 0) return false;
    }
    return brace == 0 && bracket == 0 && !inStr;
  }
}

public class AzureOpenAISettings
{
  public string? Endpoint { get; set; }
  public string? ApiKey { get; set; }
  public string? DeploymentName { get; set; }
  public bool IsConfigured => !string.IsNullOrWhiteSpace(Endpoint) && !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(DeploymentName);
}
