namespace code_review_trainer_service.CodeReviewProblems;

using System;

public record ProblemDefinition
{
  public string Purpose { get; init; }
  public string Patch { get; init; }

  public ProblemDefinition(string purpose, string patch)
  {
    if (string.IsNullOrWhiteSpace(patch))
      throw new ArgumentException("Patch must not be null or empty", nameof(patch));

    if (string.IsNullOrWhiteSpace(purpose))
      throw new ArgumentException("Purpose must not be null or empty", nameof(purpose));

    Purpose = purpose;
    Patch = patch;
  }
}
