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
      var systemPrompt = @"You are a senior C# engineer conducting code review training. Your goal is to help train developers to become better at code reviews.

IMPORTANT: Output ONLY valid, minified JSON object per the schema. ABSOLUTELY NO markdown, no backticks, no commentary outside the JSON.

Your analysis should:
1. Conduct your own thorough review of the code (up to 1000 words total response)
2. Parse the developer's review to identify what they found vs what they missed
3. Evaluate their review for clarity and actionable items
4. Provide a score from 0 (terrible) to 10 (perfect) based ONLY on the quality of their review, NOT the code quality
5. Keep the issuesDetected array comprehensive - do not truncate
6. Provide detailed feedback in the summary

CRITICAL: The overallScore should evaluate ONLY the user's review quality, not the code quality. Score based on:
- Completeness: Did they catch the important issues?
- Accuracy: Were their observations correct?
- Clarity: Were their comments clear and actionable?
- Appropriate tone: Were they constructive and professional?

The summary should include:
- What you found correctly in your review
- What critical issues you missed
- Assessment of your review quality (clarity, actionability)
- Specific suggestions for improving your review skills
- Overall assessment justifying your score (focused on review quality, not code quality)
- When appropriate, note that approving the PR would be acceptable if there are no significant issues

Remember: Not every code sample needs blocking issues. If the code is generally well-written with only minor suggestions, it's perfectly appropriate to approve the PR. Help developers understand when to approve vs when to request changes.

Address the reviewer directly using 'you' - never refer to 'the user' or 'they'. Speak directly to the person who submitted the review.";
      var userPrompt = BuildUserPrompt(request);

      var messages = new List<ChatMessage>
      {
        new SystemChatMessage(systemPrompt),
        new UserChatMessage(userPrompt)
      };

      var options = new ChatCompletionOptions
      {
        MaxOutputTokenCount = 1200,
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
    var truncatedCode = req.Code; // Don't truncate the code
    var truncatedReview = Truncate(req.UserReview, 2500); // Increase user review limit to 2500
    // Escape braces by doubling for string interpolation
    var schema = "{{ problemId, overallScore, issuesDetected:[{{id,category,title,explanation,severity}}], matchedUserPoints:[{{excerpt,matchedIssueIds,accuracy}}], missedCriticalIssueIds:[], summary }}";
    return $@"ProblemId: {req.ProblemId}

OriginalCode:
```csharp
{truncatedCode}
```

UserReview:
{truncatedReview}

Conduct a comprehensive code review analysis:
1. Perform your own thorough review of the code
2. Identify all issues in the code (populate issuesDetected with comprehensive list - do not truncate)
3. Analyze what the user found correctly (populate matchedUserPoints)
4. Identify what critical issues the user missed (populate missedCriticalIssueIds with descriptive text like ""Issue 5: Lack of Input Validation"", not just IDs)
5. Evaluate the user's review quality (clarity, actionability, completeness)
6. Provide a score from 0-10 and detailed feedback in summary (up to 1000 words)

IMPORTANT: For missedCriticalIssueIds, provide descriptive text that clearly identifies what was missed (e.g., ""Issue 3: Magic Numbers Should Be Constants"", ""Lack of Input Validation"", ""Poor Variable Naming""), not just the issue ID numbers.

Return ONLY RAW JSON (no markdown fences) matching schema: {schema}";
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
