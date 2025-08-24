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
2. CAREFULLY parse the developer's review to identify what they found vs what they missed
3. Evaluate their review for clarity and actionable items
4. Keep the issuesDetected array comprehensive - do not truncate
5. Provide detailed feedback in the summary

IMPORTANT: For scoring, include a numeric ""possibleScore"" for each item in ""issuesDetected"". Do NOT include any overall numeric totals in the model output; the server will compute totals. If you include any totals, ensure they never exceed the sum of the per-issue possibleScore values plus 2 (two points reserved for general review quality).
MUST include a boolean field in the JSON root named ""reviewQualityBonusGranted"": true or false indicating whether the reviewer earned the +2 general review quality bonus. This field is REQUIRED and must always be present (set true when the review is clear and actionable, otherwise set false). Do NOT omit this field. Optionally include the exact phrase ""Earned 2 additional points for a clear and actionable review"" in the summary paragraph when true so humans can see it. Do not vary the spelling of that phrase if you include it.

CRITICAL PARSING INSTRUCTIONS:
- Read the user's review thoroughly and look for ANY mention of issues, even if phrased differently than you would phrase them
- Input validation can be mentioned as: 'add validation', 'check for null', 'validate parameters', 'don't allow negative numbers', etc.
- Error handling can be mentioned as: 'handle exceptions', 'try-catch', 'error checking', 'what if this fails', etc.
- Performance can be mentioned as: 'inefficient', 'slow', 'optimize', 'better algorithm', etc.
- Security can be mentioned as: 'security risk', 'unsafe', 'vulnerability', 'sanitize input', etc.
- DO NOT mark something as missed if the user mentioned it in ANY reasonable form

SUMMARY FORMAT (MUST be EXACTLY TWO PARAGRAPHS separated by ONE blank LINE):
Paragraph 1 MUST start with ""Summary:"" and include: what the reviewer did well, missed critical/high-value issues, and concise justification of review quality.
Paragraph 2 MUST start with ""How you can improve:"" OR (if near-perfect) ""How to further improve:"" and provide specific, actionable guidance tied to this review's gaps. If the review is already very good and there are no actionable improvements, the second paragraph should still be present but may be a single short line such as: ""How to further improve: keep up the good work"". However, if there ARE spelling, formatting, clarity, or missing-item issues, the second paragraph must contain specific, actionable advice addressing them.

Return only the JSON matching the schema described in the user prompt. Do not add explanatory fields or markdown";
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
        int possibleScore = 0;
        if (i.TryGetProperty("possibleScore", out var ps) && ps.ValueKind == JsonValueKind.Number)
        {
          // Read as int if possible, otherwise round
          if (ps.TryGetInt32(out var intVal)) possibleScore = intVal;
          else if (ps.TryGetDouble(out var dbl)) possibleScore = (int)Math.Round(dbl);
          else possibleScore = 1;
        }
        else
        {
          // Infer possible score from severity if model didn't provide one
          possibleScore = severity.ToLowerInvariant() switch
          {
            "critical" => 3,
            "high" => 3,
            "medium" => 2,
            "low" => 1,
            "trivial" => 1,
            _ => 2
          };
        }
        issues.Add(new CodeReviewIssue(issueId, category, title, explanation, severity, possibleScore));
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

    // Compute empirical scoring: sum possible for all detected issues, and user-earned score from matched points
    // Base possible total is the sum of per-issue possible scores. The +2
    // review-quality bonus is not included in this value; it is reported via
    // ReviewQualityBonusGranted and may be added to the user's score only.
    int possibleTotal = issues.Sum(i => i.PossibleScore);
    int userTotal = 0;

    // For each matched user point, award the possibleScore for matched issues only once per issue.
    var awardedIssueIds = new HashSet<string>();
    foreach (var m in matched)
    {
      // Skip matches with low accuracy or empty matched IDs
      var accuracyNormalized = (m.Accuracy ?? string.Empty).ToLowerInvariant();
      foreach (var mid in m.MatchedIssueIds ?? Array.Empty<string>())
      {
        if (string.IsNullOrWhiteSpace(mid)) continue;
        if (awardedIssueIds.Contains(mid)) continue; // already awarded
        var issue = issues.FirstOrDefault(i => string.Equals(i.Id, mid, StringComparison.OrdinalIgnoreCase));
        if (issue == null) continue;

        // Determine award multiplier: only award if accuracy not explicitly 'incorrect' or 'false'
        bool award = !accuracyNormalized.Contains("incorrect") && !accuracyNormalized.Contains("false") && !accuracyNormalized.Contains("no");
        if (award)
        {
          userTotal += issue.PossibleScore;
          awardedIssueIds.Add(mid);
        }
      }
    }

    // Extra/penalty rules not directly represented by matched items: prefer explicit model flag, then summary cues
    bool awardedReviewBonus = false;
    // Detect presence of the required reviewQualityBonusGranted field
    bool modelProvidedBonusField = el.TryGetProperty("reviewQualityBonusGranted", out var rqbTemp);
    if (!modelProvidedBonusField)
    {
      _logger.LogWarning("Model output missing required 'reviewQualityBonusGranted' boolean. Falling back to summary heuristics. Raw: {Raw}", raw);
    }
    // First, prefer a machine-readable boolean flag from the model output
    if (modelProvidedBonusField && el.TryGetProperty("reviewQualityBonusGranted", out var rqb) && rqb.ValueKind == JsonValueKind.True)
    {
      // Respect the explicit flag in the model output when the summary also contains positive signals
      // (exact award phrase, clear/actionable mentions, or positive adjectives). Only ignore when
      // there are negative cues and no positive indicators.
      var sForFlag = summary.ToLowerInvariant();
      var negationKeywordsForFlag = new[] { "lack", "lacks", "missing", "missed", "no", "not", "doesn't", "didn't", "without", "low", "poor", "insufficient" };
      bool hasNegationForFlag = negationKeywordsForFlag.Any(k => sForFlag.Contains(k));

      var positiveIndicators = new[] { "earned 2 additional points", "earning 2 additional points", "clear and actionable", "clear, actionable", "actionable feedback", "actionable", "good", "well" };
      bool hasPositiveIndicator = positiveIndicators.Any(p => sForFlag.Contains(p));

      if (!hasNegationForFlag || hasPositiveIndicator)
      {
        userTotal += 2;
        awardedReviewBonus = true;
      }
      else
      {
        _logger.LogInformation("Ignoring reviewQualityBonusGranted=true because summary contains negation cues and no positive indicators: {Summary}", summary);
      }
    }

    // Then inspect the summary for an explicit human-readable phrase or fallback keywords
    if (!string.IsNullOrWhiteSpace(summary))
    {
      var s = summary.ToLowerInvariant();

      // Exact phrase fallback (as requested in prompt):
      if (!awardedReviewBonus && s.Contains("earned 2 additional points for a clear and actionable review"))
      {
        userTotal += 2;
        awardedReviewBonus = true;
      }

      // Last-resort fallback: keyword-based detection of clarity/actionability.
      // Only award if the summary indicates positive clarity/actionability (no nearby negation cues).
      if (!awardedReviewBonus)
      {
        var negationKeywords = new[] { "lack", "lacks", "missing", "missed", "no", "not", "doesn't", "didn't", "without", "low", "poor", "insufficient" };
        bool hasNegation = negationKeywords.Any(k => s.Contains(k));
        var positiveActionablePhrases = new[] { "clear and actionable", "clear, actionable", "actionable guidance", "actionable suggestions", "actionable items", "actionable feedback", "actionable" };
        bool hasActionable = positiveActionablePhrases.Any(p => s.Contains(p));
        if (hasActionable && !hasNegation)
        {
          userTotal += 2;
          awardedReviewBonus = true;
        }
      }

      // -1 for spelling errors or non-functional mistakes: if model mentions 'spelling' or 'typo'
      if (s.Contains("spelling") || s.Contains("typo") || s.Contains("spelling error"))
      {
        userTotal -= 1;
      }

      // Final permissive check: if summary mentions both 'clear' and 'actionable' (not necessarily as a phrase)
      // and we haven't awarded the bonus yet, grant it unless there are negation cues.
      if (!awardedReviewBonus)
      {
        bool mentionsClear = s.Contains("clear");
        bool mentionsActionable = s.Contains("actionable");
        var negationKeywords = new[] { "lack", "lacks", "missing", "missed", "no", "not", "doesn't", "didn't", "without", "low", "poor", "insufficient" };
        bool hasNegation = negationKeywords.Any(k => s.Contains(k));
        // If the summary explicitly says 'Overall, ...' along with clear+actionable, honor the positive signal
        if (mentionsClear && mentionsActionable && (!hasNegation || s.Contains("overall")))
        {
          userTotal += 2;
          awardedReviewBonus = true;
          _logger.LogInformation("Awarding review quality bonus based on permissive check (clear+actionable) for summary: {Summary}", summary);
        }
      }
    }

    // Ensure that a sample that only has minor/low issues does not penalize simple LGTM approvals.
    // If all issues are low/trivial and userTotal == 0 (i.e., matched none), consider not penalizing.
    if (possibleTotal > 0 && issues.All(i => i.PossibleScore <= 1) && userTotal <= 0)
    {
      // Treat LGTM as acceptable - award zero rather than negative or leave zero
      userTotal = Math.Max(0, userTotal);
    }

    possibleTotal = Math.Max(0, possibleTotal);
    userTotal = Math.Max(0, userTotal);
    if (userTotal > possibleTotal) userTotal = possibleTotal;

    return new CodeReviewModelResult(problemId, issues, matched, missed, summary, raw, false, null, awardedReviewBonus, UserScore: userTotal, PossibleScore: possibleTotal);
  }

  private static string BuildUserPrompt(CodeReviewRequest req)
  {
    var truncatedCode = req.Code; // Don't truncate the code
    var truncatedReview = Truncate(req.UserReview, 2500); // Increase user review limit to 2500

    // Determine language from problem ID
    var language = "csharp"; // default
    if (req.ProblemId.StartsWith("js_", StringComparison.OrdinalIgnoreCase))
    {
      language = "javascript";
    }

    // Escape braces by doubling for string interpolation
    // Extend schema to include possibleScore for each detected issue and for matched points include possibleScore
    var schema = "{{ problemId, issuesDetected:[{{id,category,title,explanation,severity,possibleScore}}], matchedUserPoints:[{{excerpt,matchedIssueIds,accuracy}}], missedCriticalIssueIds:[], reviewQualityBonusGranted, summary }}";
    return $@"ProblemId: {req.ProblemId}

OriginalCode:
```{language}
{truncatedCode}
```

UserReview:
{truncatedReview}

Conduct a comprehensive code review analysis for this {(language == "javascript" ? "JavaScript" : "C#")} code:
1. Perform your own thorough review of the code
2. Identify all issues in the code (populate issuesDetected with comprehensive list - do not truncate)
3. CAREFULLY analyze what the user found correctly - look for ANY mention of issues even if phrased differently than you would phrase them:
   - Input validation mentioned as: 'add validation', 'validate that text is not null', 'don't allow negative numbers', 'check parameters', etc.
   - Error handling mentioned as: 'handle exceptions', 'try-catch', 'error checking', 'what if this fails', etc.
   - Performance mentioned as: 'inefficient', 'slow', 'optimize', 'better algorithm', etc.
   - Security mentioned as: 'security risk', 'unsafe', 'vulnerability', 'sanitize input', etc.
4. Identify what critical issues the user missed (populate missedCriticalIssueIds with descriptive text ONLY for issues that were truly not mentioned)
  5. Evaluate the user's review quality (clarity, actionability, completeness)
6. Provide detailed feedback in summary (up to 1000 words)

IMPORTANT: 
- For missedCriticalIssueIds, provide descriptive text that clearly identifies what was missed (e.g., ""Issue 3: Magic Numbers Should Be Constants"", ""Lack of Input Validation"", ""Poor Variable Naming""), not just the issue ID numbers.
- DO NOT include an issue in missedCriticalIssueIds if the user mentioned it in ANY reasonable form
- Give the user credit for finding issues even if they described them differently than you would

Return ONLY RAW JSON (no markdown fences) matching schema: {schema}";
  }

  private static string Truncate(string s, int max) => s.Length <= max ? s : s.Substring(0, max) + "\n/* truncated */";

  private CodeReviewModelResult Fallback(string problemId, string reason, string? details = null, string? raw = null) =>
    new(
      ProblemId: problemId,
      IssuesDetected: Array.Empty<CodeReviewIssue>(),
      MatchedUserPoints: Array.Empty<CodeReviewMatchedUserPoint>(),
      MissedCriticalIssueIds: Array.Empty<string>(),
      Summary: $"Fallback: {reason} {(details ?? string.Empty)}",
      RawModelJson: raw ?? string.Empty,
      IsFallback: true,
      Error: reason + (details != null ? ": " + details : string.Empty),
      ReviewQualityBonusGranted: false,
      UserScore: 0,
      PossibleScore: 0
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
