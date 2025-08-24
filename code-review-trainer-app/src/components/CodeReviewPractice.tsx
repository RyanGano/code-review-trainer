import { useState, useCallback, useEffect } from "react";
import { useMsal } from "@azure/msal-react";
import { InteractionRequiredAuthError } from "@azure/msal-browser";
import { apiConfig } from "../authConfig";
import CodeMirror from "@uiw/react-codemirror";
import { csharp } from "@replit/codemirror-lang-csharp";
import { javascript } from "@codemirror/lang-javascript";

import "./CodeReviewPractice.less";

interface CodeReviewIssue {
  id: string;
  title?: string;
  category?: string;
  severity?: string;
  possibleScore?: number;
  explanation?: string;
}

interface CodeReviewMatchedUserPoint {
  excerpt: string;
  accuracy?: string;
  matchedIssueIds?: string[];
}

interface CodeReviewModelResult {
  problemId?: string;
  issuesDetected?: CodeReviewIssue[];
  matchedUserPoints?: CodeReviewMatchedUserPoint[];
  missedCriticalIssueIds?: string[];
  summary?: string;
  rawModelJson?: string;
  isFallback?: boolean;
  error?: string;
  userScore?: number;
  possibleScore?: number;
  reviewQualityBonusGranted?: boolean;
}

interface CodeReviewTest {
  level: string;
  language?: string;
  problem: string;
  id: string;
}

const CodeReviewPractice = () => {
  const { instance, accounts } = useMsal();
  const [currentTest, setCurrentTest] = useState<CodeReviewTest | null>(null);
  const [reviewComments, setReviewComments] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedDifficulty, setSelectedDifficulty] = useState<string>("Easy");
  const [selectedLanguage, setSelectedLanguage] = useState<string>("CSharp");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submissionResult, setSubmissionResult] =
    useState<CodeReviewModelResult | null>(null);
  // Prevent infinite re-fetch loop when backend is failing
  const [autoFetchAttempted, setAutoFetchAttempted] = useState(false);

  const MAX_REVIEW_LENGTH = 2500;
  const WARNING_THRESHOLD = 2200; // Show warning at 88% of limit

  // Helper to acquire API token with interactive fallback
  const acquireApiToken = useCallback(async (): Promise<string> => {
    if (accounts.length === 0) throw new Error("Not signed in");
    const request = {
      scopes: apiConfig.b2cScopes,
      account: accounts[0],
    } as const;
    try {
      const silent = await instance.acquireTokenSilent(request);
      return silent.accessToken;
    } catch (err: unknown) {
      const msalErr = err as { errorCode?: string };
      const needsInteraction =
        err instanceof InteractionRequiredAuthError ||
        ["consent_required", "interaction_required", "login_required"].includes(
          msalErr?.errorCode || ""
        );
      if (!needsInteraction) throw err as Error;
      const popup = await instance.acquireTokenPopup({
        ...request,
        prompt: "select_account",
      });
      return popup.accessToken;
    }
  }, [accounts, instance]);

  const fetchCodeReviewTest = useCallback(async () => {
    if (accounts.length === 0) {
      setError("You must be signed in to practice code reviews");
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      const accessToken = await acquireApiToken();

      // Call API to get a test with the selected difficulty and language
      const testResponse = await fetch(
        `${apiConfig.webApi}tests/?level=${selectedDifficulty}&language=${selectedLanguage}`,
        {
          headers: {
            Authorization: `Bearer ${accessToken}`,
          },
        }
      );

      if (!testResponse.ok) {
        throw new Error(`HTTP error! status: ${testResponse.status}`);
      }

      const testData = await testResponse.json();
      setCurrentTest(testData);
      setReviewComments(""); // Reset comments for new test
      setSubmissionResult(null); // Reset previous submission results
    } catch (error) {
      console.error("Error fetching code review test:", error);
      setError(
        error instanceof Error ? error.message : "Unknown error occurred"
      );
    } finally {
      setIsLoading(false);
    }
  }, [accounts, selectedDifficulty, selectedLanguage, acquireApiToken]);

  // Auto-fetch a code review test once when user signs in (avoid infinite retry loop on errors)
  useEffect(() => {
    if (
      accounts.length > 0 &&
      !currentTest &&
      !isLoading &&
      !autoFetchAttempted
    ) {
      setAutoFetchAttempted(true);
      fetchCodeReviewTest();
    }
  }, [
    accounts,
    currentTest,
    isLoading,
    autoFetchAttempted,
    fetchCodeReviewTest,
  ]);

  const handleSubmitReview = async () => {
    if (!currentTest || !reviewComments.trim() || accounts.length === 0) {
      return;
    }

    // Truncate review if it exceeds the limit
    const finalReview =
      reviewComments.length > MAX_REVIEW_LENGTH
        ? reviewComments.substring(0, MAX_REVIEW_LENGTH)
        : reviewComments;

    setIsSubmitting(true);
    setError(null);

    try {
      const accessToken = await acquireApiToken();

      // Submit review to backend
      const submitResponse = await fetch(
        `${apiConfig.webApi}tests/${currentTest.id}`,
        {
          method: "POST",
          headers: {
            Authorization: `Bearer ${accessToken}`,
            "Content-Type": "application/json",
          },
          body: JSON.stringify({ review: finalReview.trim() }),
        }
      );

      if (!submitResponse.ok) {
        throw new Error(`HTTP error! status: ${submitResponse.status}`);
      }

      const result = await submitResponse.json();
      setSubmissionResult(result);
    } catch (error) {
      console.error("Error submitting review:", error);
      setError(
        error instanceof Error ? error.message : "Unknown error occurred"
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleNewTest = () => {
    fetchCodeReviewTest();
  };

  // When no test loaded yet (either fresh start or after an error), show the start panel
  if (!currentTest && !isLoading) {
    return (
      <div className="practice-start">
        <h2>Ready to practice reviewing code?</h2>
        <p>
          Choose a difficulty level and click <code>Get New Code Sample</code>{" "}
          to get a new test.
        </p>

        <div className="common-selection">
          <label htmlFor="difficulty-select">Difficulty Level:</label>
          <select
            id="difficulty-select"
            value={selectedDifficulty}
            onChange={(e) => setSelectedDifficulty(e.target.value)}
            className="common-dropdown"
          >
            <option value="Easy">Easy</option>
            <option value="Medium">Medium</option>
          </select>
        </div>

        <div className="common-selection">
          <label htmlFor="language-select">Language:</label>
          <select
            id="language-select"
            value={selectedLanguage}
            onChange={(e) => setSelectedLanguage(e.target.value)}
            className="common-dropdown"
          >
            <option value="CSharp">C#</option>
            <option value="JavaScript">JavaScript</option>
          </select>
        </div>

        <button
          className="start-button"
          onClick={() => {
            // Allow another auto attempt after manual retry
            setAutoFetchAttempted(true); // mark attempted; manual triggers explicit fetch
            fetchCodeReviewTest();
          }}
          disabled={accounts.length === 0 || isLoading}
        >
          {isLoading ? "Loading…" : currentTest ? "Loaded" : "Start Practicing"}
        </button>
        {accounts.length === 0 && (
          <p className="signin-required">Please sign in to start practicing</p>
        )}
        {error && (
          <div className="error-message" style={{ marginTop: "1rem" }}>
            Error loading sample: {error}
            <div>
              <button
                onClick={() => {
                  setError(null);
                  fetchCodeReviewTest();
                }}
                disabled={isLoading}
                style={{ marginTop: "0.5rem" }}
              >
                {isLoading ? "Retrying…" : "Retry"}
              </button>
            </div>
          </div>
        )}
      </div>
    );
  }

  return (
    <div className="code-review-practice">
      <div className="practice-header">
        <h2>
          Code Review Practice - {currentTest?.level || selectedDifficulty}{" "}
          Level - {currentTest?.language || selectedLanguage}
        </h2>
        <div className="header-controls">
          <div className="common-selection">
            <label htmlFor="difficulty-select-active">Difficulty:</label>
            <select
              id="difficulty-select-active"
              value={selectedDifficulty}
              onChange={(e) => setSelectedDifficulty(e.target.value)}
              className="common-dropdown"
            >
              <option value="Easy">Easy</option>
              <option value="Medium">Medium</option>
            </select>
          </div>
          <div className="common-selection">
            <label htmlFor="language-select-active">Language:</label>
            <select
              id="language-select-active"
              value={selectedLanguage}
              onChange={(e) => setSelectedLanguage(e.target.value)}
              className="common-dropdown"
            >
              <option value="CSharp">C#</option>
              <option value="JavaScript">JavaScript</option>
            </select>
          </div>
          <button onClick={handleNewTest} disabled={isLoading}>
            Get New Code Sample
          </button>
        </div>
      </div>

      {isLoading && <div className="loading-message">Loading code sample…</div>}

      {error && <div className="error-message">Error: {error}</div>}

      {currentTest && (
        <div>
          <div className="code-section">
            <h3>Code to Review:</h3>
            <div className="code-viewer-container">
              <CodeMirror
                value={currentTest.problem}
                extensions={[
                  selectedLanguage === "JavaScript" ? javascript() : csharp(),
                ]}
                editable={false}
                basicSetup={{
                  lineNumbers: true,
                  foldGutter: true,
                  dropCursor: false,
                  allowMultipleSelections: false,
                  indentOnInput: false,
                  bracketMatching: true,
                  closeBrackets: false,
                  autocompletion: false,
                  highlightSelectionMatches: false,
                  searchKeymap: false,
                }}
              />
            </div>
          </div>

          <div className="review-section">
            <h3>Your Review Comments:</h3>
            <textarea
              value={reviewComments}
              onChange={(e) => setReviewComments(e.target.value)}
              placeholder="Enter your code review comments here. What issues do you see? What would you suggest to improve this code?"
              className="review-textarea"
              maxLength={MAX_REVIEW_LENGTH}
            />
            <div className="character-count">
              <span
                className={`count ${
                  reviewComments.length >= WARNING_THRESHOLD ? "warning" : ""
                } ${reviewComments.length >= MAX_REVIEW_LENGTH ? "limit" : ""}`}
              >
                {reviewComments.length} / {MAX_REVIEW_LENGTH} characters
              </span>
              {reviewComments.length >= WARNING_THRESHOLD &&
                reviewComments.length < MAX_REVIEW_LENGTH && (
                  <span className="warning-text">
                    Approaching character limit
                  </span>
                )}
              {reviewComments.length >= MAX_REVIEW_LENGTH && (
                <span className="limit-text">Character limit reached</span>
              )}
            </div>
          </div>

          <div className="submit-section">
            <button
              onClick={handleSubmitReview}
              disabled={!reviewComments.trim() || isSubmitting}
              className={`submit-button ${
                reviewComments.trim() && !isSubmitting ? "enabled" : "disabled"
              }`}
            >
              {isSubmitting ? "Submitting…" : "Submit Review"}
            </button>
            {isSubmitting && (
              <div className="loading-indicator">
                Please wait while we evaluate your review…
              </div>
            )}
          </div>

          {submissionResult && (
            <div className="submission-result">
              <h4>Review Evaluation</h4>
              {submissionResult.isFallback && (
                <div className="warning">
                  Fallback (no AI evaluation). {submissionResult.error}
                </div>
              )}
              {/* overallScore removed - using server-calculated score instead */}
              <div className="score-row">
                {(submissionResult.userScore !== undefined ||
                  submissionResult.possibleScore !== undefined) && (
                  <p className="score">
                    {`Score: ${submissionResult.userScore ?? 0} / ${
                      submissionResult.possibleScore ?? 0
                    }`}
                  </p>
                )}
                {submissionResult.reviewQualityBonusGranted && (
                  <div
                    className="bonus-badge"
                    title={"Awarded +2 for a clear and actionable review"}
                    aria-label={"Awarded +2 for a clear and actionable review"}
                    role="img"
                  >
                    <span className="bonus-symbol">★</span>
                    <span className="bonus-value">+2</span>
                  </div>
                )}
              </div>
              {submissionResult.summary && (
                <div>
                  <pre className="summary-block">
                    {submissionResult.summary}
                  </pre>
                </div>
              )}
              {submissionResult.issuesDetected &&
                submissionResult.issuesDetected.length > 0 && (
                  <div className="issues-section">
                    <h5>
                      Detected Issues ({submissionResult.issuesDetected.length})
                    </h5>
                    <ul>
                      {submissionResult.issuesDetected.map(
                        (i: CodeReviewIssue) => (
                          <li key={i.id}>
                            <strong>{i.title || i.id}</strong> [{i.category}/
                            {i.severity}] – {i.explanation}
                            {i.possibleScore !== undefined && (
                              <span className="possible-score">
                                {" "}
                                (Possible: {i.possibleScore})
                              </span>
                            )}
                          </li>
                        )
                      )}
                    </ul>
                  </div>
                )}
              {submissionResult.missedCriticalIssueIds &&
                submissionResult.missedCriticalIssueIds.length > 0 && (
                  <div className="missed-section">
                    <h5>Missed Critical Issues</h5>
                    <ul>
                      {submissionResult.missedCriticalIssueIds.map(
                        (id: string) => (
                          <li key={id}>{id}</li>
                        )
                      )}
                    </ul>
                  </div>
                )}
              {submissionResult.matchedUserPoints &&
                submissionResult.matchedUserPoints.length > 0 && (
                  <div className="matched-section">
                    <h5>Your Points Matched</h5>
                    <ul>
                      {submissionResult.matchedUserPoints.map(
                        (p: CodeReviewMatchedUserPoint, idx: number) => (
                          <li key={idx}>
                            <em>{p.excerpt}</em> → {p.accuracy} (Matches:{" "}
                            {p.matchedIssueIds?.join(", ") || "—"})
                          </li>
                        )
                      )}
                    </ul>
                  </div>
                )}
              {submissionResult.error && !submissionResult.isFallback && (
                <div className="error-message">{submissionResult.error}</div>
              )}
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default CodeReviewPractice;
