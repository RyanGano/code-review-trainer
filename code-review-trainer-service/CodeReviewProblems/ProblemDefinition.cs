namespace code_review_trainer_service.CodeReviewProblems;

public record ProblemDefinition
{
  public string Purpose { get; init; }
  public string Patch { get; init; }
  public StoredReview Review { get; init; }

  public ProblemDefinition(string purpose, string patch, StoredReview review)
  {
    if (string.IsNullOrWhiteSpace(patch))
      throw new ArgumentException("Patch must not be null or empty", nameof(patch));

    if (string.IsNullOrWhiteSpace(purpose))
      throw new ArgumentException("Purpose must not be null or empty", nameof(purpose));

    ArgumentNullException.ThrowIfNull(review);

    Purpose = purpose;
    Patch = patch;
    Review = review;
  }
}
