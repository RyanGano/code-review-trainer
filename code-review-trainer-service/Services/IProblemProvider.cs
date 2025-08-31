using code_review_trainer_service.CodeReviewProblems;

namespace code_review_trainer_service.Services;

public interface IProblemProvider
{
  DifficultyLevel Difficulty { get; }
  Language Language { get; }
  int Count { get; }
  string GetProblemByIndex(int index);
  string GetPurposeByIndex(int index);
  CodeReviewProblem GetRandomProblemWithId();
}
