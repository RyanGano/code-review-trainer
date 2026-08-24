using code_review_trainer_service.CodeReviewProblems;

namespace code_review_trainer_service.Services;

/// <summary>
/// Everything the service knows about a single problem, including the reference review.
/// </summary>
public record ProblemDetails(string Id, string Code, string Purpose, Language Language, StoredReview Review);

public interface IProblemRepository
{
  ProblemDetails? Get(string id);
}

public class ProblemRepository(IEnumerable<IProblemProvider> providers) : IProblemRepository
{
  private readonly IEnumerable<IProblemProvider> _providers = providers;

  public ProblemDetails? Get(string id)
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

    if (language is null) return null;

    var difficulty = parts[1].ToLowerInvariant() switch
    {
      "easy" => DifficultyLevel.Easy,
      "medium" => DifficultyLevel.Medium,
      _ => (DifficultyLevel?)null
    };

    if (difficulty is null) return null;

    if (!int.TryParse(parts[2], out var oneBased) || oneBased <= 0) return null;
    var index = oneBased - 1;

    var provider = _providers.FirstOrDefault(p => p.Language == language && p.Difficulty == difficulty);
    if (provider is null) return null;
    if (index < 0 || index >= provider.Count) return null;

    var review = provider.GetReviewByIndex(index);
    if (review is null) return null;

    return new ProblemDetails(
      id,
      provider.GetProblemByIndex(index),
      provider.GetPurposeByIndex(index),
      language.Value,
      review);
  }
}
