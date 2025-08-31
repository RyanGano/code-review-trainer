namespace code_review_trainer_service.CodeReviewProblems;

// Data structure to hold a code review problem with its unique identifier
public class CodeReviewProblem
{
  public string Id { get; set; } = string.Empty;
  public string Problem { get; set; } = string.Empty;
  public Language Language { get; set; } = Language.CSharp;
  // Optional unified patch text (server-provided). Lines prefixed with --- for removed, +++ for added.
  public string? Patch { get; set; }
  // Short purpose/description shown in the UI (e.g., what to look for).
  public string Purpose { get; set; } = string.Empty;
}
