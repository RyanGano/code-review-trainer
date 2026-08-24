using code_review_trainer_service.CodeReviewProblems;

namespace code_review_trainer_service.Services;

/// <summary>
/// A grading request. <paramref name="Review"/> is the stored reference review for the problem;
/// the model grades the user's write-up against it instead of reviewing the code itself.
/// </summary>
public record CodeReviewRequest(
    string ProblemId,
    string Code,
    string UserReview,
    string PatchPurpose,
    StoredReview Review,
    bool? UserShippabilityAssessment = null);

public record CodeReviewIssue(
    string Id,
    string Category,
    string Title,
    string Explanation,
    string Severity,
    int PossibleScore
);

/// <summary>
/// One point the developer made, as the grader read it.
/// </summary>
/// <param name="Excerpt">The developer's own words, so they can see what was graded.</param>
/// <param name="MatchedIssueIds">Reference issue ids this point refers to; empty when it matches none.</param>
/// <param name="Accuracy">correct, partial, valid_but_unlisted or incorrect.</param>
/// <param name="Comment">One sentence to the developer explaining the rating.</param>
public record CodeReviewMatchedUserPoint(
    string Excerpt,
    string[] MatchedIssueIds,
    string Accuracy,
    string Comment
);

public record CodeReviewModelResult(
    string ProblemId,
    IReadOnlyList<CodeReviewIssue> IssuesDetected,
    IReadOnlyList<CodeReviewMatchedUserPoint> MatchedUserPoints,
    IReadOnlyList<string> MissedCriticalIssueIds,
    string Summary,
    string RawModelJson,
    string RecommendedCode,
    bool IsFallback,
    string? Error,
    bool SpellingProblemsDetected,
    bool ReviewQualityBonusGranted,
    int UserScore,
    int PossibleScore,
    ReviewStatus ReviewStatus
);

public interface ICodeReviewModel
{
    Task<CodeReviewModelResult> ReviewAsync(CodeReviewRequest request, CancellationToken ct = default);
}
