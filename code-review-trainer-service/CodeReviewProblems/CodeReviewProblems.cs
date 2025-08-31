namespace code_review_trainer_service.CodeReviewProblems;

public abstract class CodeReviewProblems : Services.IProblemProvider
{
  protected readonly ProblemDefinition[] Problems;
  protected readonly Language Language;
  protected readonly string IdPrefix;
  protected readonly DifficultyLevel Difficulty;

  private readonly Random _random = new Random();

  protected CodeReviewProblems(ProblemDefinition[] problems, Language language, string idPrefix, DifficultyLevel difficulty)
  {
    Problems = problems ?? Array.Empty<ProblemDefinition>();
    Language = language;
    IdPrefix = idPrefix ?? string.Empty;
    Difficulty = difficulty;
  }

  public CodeReviewProblem GetRandomProblemWithId()
  {
    if (Problems.Length == 0)
    {
      return new CodeReviewProblem
      {
        Id = $"{IdPrefix}_000",
        Purpose = string.Empty,
        Language = Language
      };
    }

    var index = _random.Next(Problems.Length);
    var def = Problems[index];
    return new CodeReviewProblem
    {
      Id = $"{IdPrefix}_{index + 1:D3}",
      Patch = def.Patch,
      Purpose = def.Purpose ?? string.Empty,
      Language = Language
    };
  }

  public int Count => Problems.Length;

  public string GetProblemByIndex(int index)
  {
    if (index < 0 || index >= Problems.Length) return string.Empty;
    return Problems[index].Patch ?? string.Empty;
  }

  public string GetPurposeByIndex(int index)
  {
    if (index < 0 || index >= Problems.Length) return string.Empty;
    return Problems[index].Purpose ?? string.Empty;
  }

  // IProblemProvider implementation
  DifficultyLevel Services.IProblemProvider.Difficulty => Difficulty;
  Language Services.IProblemProvider.Language => Language;
  int Services.IProblemProvider.Count => Count;
  string Services.IProblemProvider.GetProblemByIndex(int index) => GetProblemByIndex(index);
  string Services.IProblemProvider.GetPurposeByIndex(int index) => GetPurposeByIndex(index);
  CodeReviewProblem Services.IProblemProvider.GetRandomProblemWithId() => GetRandomProblemWithId();
}
