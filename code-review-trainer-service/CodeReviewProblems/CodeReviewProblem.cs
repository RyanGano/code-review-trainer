namespace code_review_trainer_service.CodeReviewProblems;

public class CodeReviewProblem
{
  public string Id { get; set; } = string.Empty;
  public string? Patch { get; set; }
  public string Purpose { get; set; } = string.Empty;
  public Language Language { get; set; }
}
