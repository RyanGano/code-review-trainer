namespace code_review_trainer_service.Services;

public record CodeReviewRequest(string ProblemId, string Code, string UserReview);

public record CodeReviewIssue(
    string Id,
    string Category,
    string Title,
    string Explanation,
    string Severity
);

public record CodeReviewMatchedUserPoint(
    string Excerpt,
    string[] MatchedIssueIds,
    string Accuracy
);

public record CodeReviewModelResult(
    string ProblemId,
    double OverallScore,
    IReadOnlyList<CodeReviewIssue> IssuesDetected,
    IReadOnlyList<CodeReviewMatchedUserPoint> MatchedUserPoints,
    IReadOnlyList<string> MissedCriticalIssueIds,
    string Summary,
    string RawModelJson,
    bool IsFallback,
    string? Error
);

public interface ICodeReviewModel
{
  Task<CodeReviewModelResult> ReviewAsync(CodeReviewRequest request, CancellationToken ct = default);
}
