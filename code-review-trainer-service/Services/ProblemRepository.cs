using code_review_trainer_service.CodeReviewProblems;

namespace code_review_trainer_service.Services;

public interface IProblemRepository
{
  (string Id, string Code)? Get(string id);
}

public class ProblemRepository : IProblemRepository
{
  public (string Id, string Code)? Get(string id)
  {
    if (string.IsNullOrWhiteSpace(id)) return null;
    
    // Handle new format: {lang}_{difficulty}_{index}
    if (id.StartsWith("cs_easy_", StringComparison.OrdinalIgnoreCase))
    {
      if (TryParseIndex(id, "cs_easy_", out var index, EasyCodeReviewProblems.Count))
      {
        return (id, EasyCodeReviewProblems.GetProblemByIndex(index));
      }
    }
    else if (id.StartsWith("cs_medium_", StringComparison.OrdinalIgnoreCase))
    {
      if (TryParseIndex(id, "cs_medium_", out var index, MediumCodeReviewProblems.Count))
      {
        return (id, MediumCodeReviewProblems.GetProblemByIndex(index));
      }
    }
    else if (id.StartsWith("js_easy_", StringComparison.OrdinalIgnoreCase))
    {
      if (TryParseIndex(id, "js_easy_", out var index, EasyJavaScriptCodeReviewProblems.Count))
      {
        return (id, EasyJavaScriptCodeReviewProblems.GetProblemByIndex(index));
      }
    }
    else if (id.StartsWith("js_medium_", StringComparison.OrdinalIgnoreCase))
    {
      if (TryParseIndex(id, "js_medium_", out var index, MediumJavaScriptCodeReviewProblems.Count))
      {
        return (id, MediumJavaScriptCodeReviewProblems.GetProblemByIndex(index));
      }
    }
    // Backward compatibility: handle old format
    else if (id.StartsWith("easy_", StringComparison.OrdinalIgnoreCase))
    {
      if (TryParseIndex(id, "easy_", out var index, EasyCodeReviewProblems.Count))
      {
        return (id, EasyCodeReviewProblems.GetProblemByIndex(index));
      }
    }
    else if (id.StartsWith("medium_", StringComparison.OrdinalIgnoreCase))
    {
      if (TryParseIndex(id, "medium_", out var index, MediumCodeReviewProblems.Count))
      {
        return (id, MediumCodeReviewProblems.GetProblemByIndex(index));
      }
    }
    return null;
  }

  private static bool TryParseIndex(string id, string prefix, out int index, int max)
  {
    index = -1;
    var part = id.Substring(prefix.Length);
    if (int.TryParse(part, out var oneBased))
    {
      index = oneBased - 1;
      return index >= 0 && index < max;
    }
    return false;
  }
}
