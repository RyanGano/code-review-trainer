namespace code_review_trainer_service.Services;

public record CodeReviewRequest(string ProblemId, string Code, string UserReview, string PatchPurpose);

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
    int PossibleScore
);

public interface ICodeReviewModel
{
    Task<CodeReviewModelResult> ReviewAsync(CodeReviewRequest request, CancellationToken ct = default);
}
