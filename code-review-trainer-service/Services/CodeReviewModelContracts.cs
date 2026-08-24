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

public record CodeReviewMatchedUserPoint(
    string Excerpt,
    string[] MatchedIssueIds,
    string Accuracy
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
