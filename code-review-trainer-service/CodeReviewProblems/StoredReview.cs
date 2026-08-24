namespace code_review_trainer_service.CodeReviewProblems;

/// <summary>
/// The verdict a reviewer should reach for a problem. Stored alongside each example so the
/// service never has to ask the model to re-review the same code.
/// </summary>
public enum ReviewStatus
{
  /// <summary>The patch is good enough to merge as-is.</summary>
  Approve,

  /// <summary>The patch has issues that must be addressed before it can be merged.</summary>
  Reject
}

/// <summary>
/// A single issue the reference review found in a problem's patch.
/// </summary>
/// <param name="Id">Stable identifier, unique within the problem (used to match user points).</param>
/// <param name="Category">Short bucket such as Correctness, Security, Performance, Maintainability, Style.</param>
/// <param name="Title">One-line statement of the defect.</param>
/// <param name="Explanation">Why it is a defect and what it causes.</param>
/// <param name="Severity">critical, high, medium, low or trivial.</param>
/// <param name="PossibleScore">Points awarded when the user finds this issue.</param>
public record StoredReviewIssue(
  string Id,
  string Category,
  string Title,
  string Explanation,
  string Severity,
  int PossibleScore
);

/// <summary>
/// The reference code review for a problem: the verdict, the issues a good reviewer should
/// find, and the code that fixes them.
/// </summary>
public record StoredReview
{
  public ReviewStatus Status { get; init; }
  public IReadOnlyList<StoredReviewIssue> Issues { get; init; }
  public string RecommendedCode { get; init; }

  public StoredReview(ReviewStatus status, IReadOnlyList<StoredReviewIssue> issues, string recommendedCode = "")
  {
    ArgumentNullException.ThrowIfNull(issues);

    if (status == ReviewStatus.Reject && issues.Count == 0)
      throw new ArgumentException("A rejected review must list at least one issue", nameof(issues));

    if (issues.Select(i => i.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() != issues.Count)
      throw new ArgumentException("Issue ids must be unique within a review", nameof(issues));

    foreach (var issue in issues)
    {
      if (string.IsNullOrWhiteSpace(issue.Id))
        throw new ArgumentException("Issue id must not be null or empty", nameof(issues));
      if (string.IsNullOrWhiteSpace(issue.Title))
        throw new ArgumentException($"Issue '{issue.Id}' must have a title", nameof(issues));
      if (string.IsNullOrWhiteSpace(issue.Explanation))
        throw new ArgumentException($"Issue '{issue.Id}' must have an explanation", nameof(issues));
      if (!ValidSeverities.Contains(issue.Severity))
        throw new ArgumentException($"Issue '{issue.Id}' has unknown severity '{issue.Severity}'", nameof(issues));
      if (issue.PossibleScore is < 1 or > 3)
        throw new ArgumentException($"Issue '{issue.Id}' must have a possible score between 1 and 3", nameof(issues));
    }

    Status = status;
    Issues = issues;
    RecommendedCode = recommendedCode ?? string.Empty;
  }

  private static readonly HashSet<string> ValidSeverities =
    new(["critical", "high", "medium", "low", "trivial"], StringComparer.OrdinalIgnoreCase);

  /// <summary>Total points available for this problem.</summary>
  public int PossibleScore => Issues.Sum(i => i.PossibleScore);
}
