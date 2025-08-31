using code_review_trainer_service.CodeReviewProblems;

namespace code_review_trainer_service.Services;

public interface IProblemRepository
{
  (string Id, string Code, string Purpose, Language Language)? Get(string id);
}

public class ProblemRepository : IProblemRepository
{
  private readonly IEnumerable<IProblemProvider> _providers;

  public ProblemRepository(IEnumerable<IProblemProvider> providers)
  {
    _providers = providers;
  }

  public (string Id, string Code, string Purpose, Language Language)? Get(string id)
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

    var provider = _providers.FirstOrDefault(p => p.Language == language && p.Difficulty == difficulty);
    if (provider == null) return null;
    if (index < 0 || index >= provider.Count) return null;
    return (id, provider.GetProblemByIndex(index), provider.GetPurposeByIndex(index), language.Value);
  }
}
