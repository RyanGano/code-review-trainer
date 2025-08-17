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
    
    // Handle format: {lang}_{difficulty}_{index}
    var language = parts[0].ToLowerInvariant() switch
    {
      "cs" => Language.CSharp,
      "js" => Language.JavaScript,
      _ => throw new ArgumentException($"Invalid language identifier '{parts[0]}'. Supported languages are: cs, js", nameof(id))
    };
    
    var difficulty = parts[1].ToLowerInvariant() switch
    {
      "easy" => DifficultyLevel.Easy,
      "medium" => DifficultyLevel.Medium,
      _ => throw new ArgumentException($"Invalid difficulty level '{parts[1]}'. Supported difficulty levels are: easy, medium", nameof(id))
    };
    
    if (!int.TryParse(parts[2], out var oneBased) || oneBased <= 0) return null;
    var index = oneBased - 1;
    
    return (language, difficulty) switch
    {
      (Language.CSharp, DifficultyLevel.Easy) when index < EasyCodeReviewProblems.Count => 
        (id, EasyCodeReviewProblems.GetProblemByIndex(index)),
      (Language.CSharp, DifficultyLevel.Medium) when index < MediumCodeReviewProblems.Count => 
        (id, MediumCodeReviewProblems.GetProblemByIndex(index)),
      (Language.JavaScript, DifficultyLevel.Easy) when index < EasyJavaScriptCodeReviewProblems.Count => 
        (id, EasyJavaScriptCodeReviewProblems.GetProblemByIndex(index)),
      (Language.JavaScript, DifficultyLevel.Medium) when index < MediumJavaScriptCodeReviewProblems.Count => 
        (id, MediumJavaScriptCodeReviewProblems.GetProblemByIndex(index)),
      _ => null
    };
  }
}
