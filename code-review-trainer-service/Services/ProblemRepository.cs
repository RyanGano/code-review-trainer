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

    var parts = id.Split('_');
    if (parts.Length != 3) return null;

    var language = parts[0].ToLowerInvariant() switch
    {
      "cs" => Language.CSharp,
      "js" => Language.JavaScript,
      "ts" => Language.TypeScript,
      _ => (Language?)null
    };

    if (language == null) return null;

    var difficulty = parts[1].ToLowerInvariant() switch
    {
      "easy" => DifficultyLevel.Easy,
      "medium" => DifficultyLevel.Medium,
      _ => (DifficultyLevel?)null
    };

    if (difficulty == null) return null;

    if (!int.TryParse(parts[2], out var oneBased) || oneBased <= 0) return null;
    var index = oneBased - 1;

    return (language, difficulty) switch
    {
      (Language.CSharp, DifficultyLevel.Easy) when index < EasyCSharpCodeReviewProblems.Count =>
        (id, EasyCSharpCodeReviewProblems.GetProblemByIndex(index)),
      (Language.CSharp, DifficultyLevel.Medium) when index < MediumCSharpCodeReviewProblems.Count =>
        (id, MediumCSharpCodeReviewProblems.GetProblemByIndex(index)),
      (Language.JavaScript, DifficultyLevel.Easy) when index < EasyJavaScriptCodeReviewProblems.Count =>
        (id, EasyJavaScriptCodeReviewProblems.GetProblemByIndex(index)),
      (Language.JavaScript, DifficultyLevel.Medium) when index < MediumJavaScriptCodeReviewProblems.Count =>
        (id, MediumJavaScriptCodeReviewProblems.GetProblemByIndex(index)),
      (Language.TypeScript, DifficultyLevel.Easy) when index < EasyTypeScriptCodeReviewProblems.Count =>
        (id, EasyTypeScriptCodeReviewProblems.GetProblemByIndex(index)),
      (Language.TypeScript, DifficultyLevel.Medium) when index < MediumTypeScriptCodeReviewProblems.Count =>
        (id, MediumTypeScriptCodeReviewProblems.GetProblemByIndex(index)),
      _ => null
    };
  }
}
