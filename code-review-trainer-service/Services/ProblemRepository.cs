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
    if (id.StartsWith("easy_", StringComparison.OrdinalIgnoreCase))
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
