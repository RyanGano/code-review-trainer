import { useState, useCallback, useEffect } from "react";
import { useMsal } from "@azure/msal-react";
import { InteractionRequiredAuthError } from "@azure/msal-browser";
import { apiConfig } from "../authConfig";
import CodeMirror from "@uiw/react-codemirror";
import { csharp } from "@replit/codemirror-lang-csharp";
import { javascript } from "@codemirror/lang-javascript";
import { BinocularsFill } from "react-bootstrap-icons";

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
  recommendedCode?: string | null;
  isFallback?: boolean;
  error?: string;
  // New: model (server) indicates spelling/typo problems found in the user's review
  spellingProblemsDetected?: boolean;
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
  interface ExplainResponse {
    explanation: string;
    examples?: string;
  }
  const [explanations, setExplanations] = useState<
    Record<string, ExplainResponse>
  >({});
  const [explainLoading, setExplainLoading] = useState<Record<string, boolean>>(
    {}
  );
  // Per-issue short error messages when explanation fetch fails
  const [explainErrors, setExplainErrors] = useState<
    Record<string, string | null>
  >({});
  // Track per-issue visibility of already-fetched explanations so users can hide/show
  const [shownExplanations, setShownExplanations] = useState<
    Record<string, boolean>
  >({});
  // Prevent infinite re-fetch loop when backend is failing
  const [autoFetchAttempted, setAutoFetchAttempted] = useState(false);
  // Show/hide state for recommended code snippet
  const [showRecommendedCode, setShowRecommendedCode] = useState(false);

  const MAX_REVIEW_LENGTH = 2500;
  const WARNING_THRESHOLD = 2200;

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
      const errorCode = msalErr?.errorCode || "";
      const needsInteraction =
        err instanceof InteractionRequiredAuthError ||
        ["consent_required", "interaction_required", "login_required"].includes(
          errorCode
        );
      if (!needsInteraction) throw err as Error;

      // If consent is required, explicitly request consent to show the "Accept" dialog
      const promptType =
        errorCode === "consent_required" ? "consent" : "select_account";
      try {
        const popup = await instance.acquireTokenPopup({
          ...request,
          prompt: promptType,
        });
        return popup.accessToken;
      } catch (popupErr: unknown) {
        // Surface a clearer message when users can't grant consent (e.g. admin required)
        const pe = popupErr as { errorCode?: string; message?: string };
        const pcode = pe?.errorCode || "";
        if (pcode === "consent_required" || pcode === "access_denied") {
          throw new Error(
            "Consent is required to call the API. Your tenant may require an administrator to grant consent. Please contact your IT admin or use an account with proper permissions."
          );
        }
        throw popupErr as Error;
      }
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
      setReviewComments("");
      // Clear previous submission results and any per-issue explanations
      setSubmissionResult(null);
      setExplanations({});
      setExplainLoading({});
      setShownExplanations({});
      setError(null);
    } catch (err) {
      console.error("Error fetching code review test:", err);
      setError(err instanceof Error ? err.message : String(err));
      setCurrentTest(null);
    } finally {
      setIsLoading(false);
    }
  }, [accounts, selectedDifficulty, selectedLanguage, acquireApiToken]);

  // Auto-load a test once after sign-in to streamline first-run experience.
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

    const finalReview =
      reviewComments.length > MAX_REVIEW_LENGTH
        ? reviewComments.substring(0, MAX_REVIEW_LENGTH)
        : reviewComments;

    setIsSubmitting(true);
    setError(null);

    try {
      const accessToken = await acquireApiToken();

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
      // Replace submission result and clear any stale explanations from prior runs
      setSubmissionResult(result);
      setExplanations({});
      setExplainLoading({});
      setShownExplanations({});
    } catch (error) {
      console.error("Error submitting review:", error);
      setError(error instanceof Error ? error.message : String(error));
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleNewTest = () => {
    fetchCodeReviewTest();
  };

  // Displayed possible score should reflect AI-detected items when available
  // (so the UI shows the per-issue totals like 10). If issues aren't present,
  // fall back to the backend-provided possibleScore.
  const getDisplayedPossibleScore = (r: CodeReviewModelResult | null) => {
    if (!r) return 0;
    if (r.issuesDetected && r.issuesDetected.length > 0) {
      return r.issuesDetected.reduce((sum, issue) => {
        const v = (issue as CodeReviewIssue).possibleScore;
        return sum + (typeof v === "number" ? v : 0);
      }, 0);
    }
    return typeof r.possibleScore === "number" ? r.possibleScore : 0;
  };

  // Displayed user score will generally come from the server (authoritative).
  // However, when the UI chooses to show the per-issue possible total (above),
  // subtract the review-quality bonus from the displayed user score so the
  // fraction matches the listed issues. The badge still indicates the bonus.
  const getDisplayedUserScore = (r: CodeReviewModelResult | null) => {
    if (!r) return 0;

    // Prefer the server-provided userScore when available
    const serverUser = typeof r.userScore === "number" ? r.userScore : 0;
    const hasIssues = r.issuesDetected && r.issuesDetected.length > 0;

    // If we are showing per-issue possible totals, subtract the review-quality
    // bonus from the displayed user score so the fraction aligns with the
    // listed issue totals (the UI still shows a badge for the bonus).
    if (hasIssues) {
      const bonus = r.reviewQualityBonusGranted ? 2 : 0;
      const adjusted = Math.max(0, serverUser - bonus);
      return adjusted;
    }

    // No per-issue list present: just show the authoritative server user score.
    return serverUser;
  };

  // Prefer machine-readable flag from server indicating spelling problems.
  // The frontend should display what the backend provides rather than
  // attempting to parse or infer from free-form text.
  const hasSpellingProblems = (r: CodeReviewModelResult | null) => {
    if (!r) return false;
    return !!r.spellingProblemsDetected;
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
            <option value="TypeScript">TypeScript</option>
          </select>
        </div>

        <button
          className="start-button"
          onClick={() => {
            // Allow another auto attempt after manual retry
            setAutoFetchAttempted(true);
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
              <option value="TypeScript">TypeScript</option>
            </select>
          </div>
          <button onClick={handleNewTest} disabled={isLoading}>
            {isLoading ? (
              <>
                Getting sample… <span className="spinner" aria-hidden="true" />
              </>
            ) : (
              "Get New Code Sample"
            )}
          </button>
        </div>
      </div>

      {error && <div className="error-message">Error: {error}</div>}

      {currentTest && (
        <div>
          <div className="code-section">
            <h3>Code to Review:</h3>
            <div className="code-viewer-container">
              <CodeMirror
                value={currentTest.problem}
                extensions={[
                  selectedLanguage === "JavaScript" ||
                  selectedLanguage === "TypeScript"
                    ? javascript()
                    : csharp(),
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
            {/* Recommended code displayed under the original code viewer for easy comparison */}
            {submissionResult && (
              <div className="recommended-code-section below-code">
                {submissionResult.recommendedCode &&
                submissionResult.recommendedCode.length > 0 ? (
                  <>
                    <button
                      className="toggle-recommended-code"
                      onClick={() => setShowRecommendedCode((s) => !s)}
                      aria-expanded={showRecommendedCode}
                    >
                      {showRecommendedCode ? (
                        <>
                          <BinocularsFill /> Hide Recommended Code
                        </>
                      ) : (
                        <>
                          <BinocularsFill /> Recommended Code
                        </>
                      )}
                    </button>

                    {/* Keep the block mounted to allow CSS transitions; toggle open/closed class */}
                    <div
                      className={`recommended-code-block ${
                        showRecommendedCode ? "open" : "closed"
                      }`}
                      aria-hidden={!showRecommendedCode}
                    >
                      <CodeMirror
                        value={submissionResult.recommendedCode}
                        extensions={[
                          selectedLanguage === "JavaScript"
                            ? javascript()
                            : csharp(),
                        ]}
                        editable={false}
                        basicSetup={{
                          lineNumbers: true,
                          foldGutter: true,
                        }}
                      />
                    </div>
                  </>
                ) : (
                  <div className="no-recommendation">
                    No code recommendation was provided by the AI for this
                    sample.
                  </div>
                )}
              </div>
            )}
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

              <div className="score-row">
                {(submissionResult.userScore !== undefined ||
                  submissionResult.possibleScore !== undefined) && (
                  <p className="score">
                    {`Score: ${getDisplayedUserScore(
                      submissionResult
                    )} / ${getDisplayedPossibleScore(submissionResult)}`}
                  </p>
                )}
                {submissionResult.reviewQualityBonusGranted && (
                  <div
                    className="bonus-badge"
                    title={"Awarded for a clear and actionable review"}
                    aria-label={"Well-written review badge"}
                    role="img"
                  >
                    <span className="bonus-symbol">★</span>
                    <span className="bonus-text">Well-written review</span>
                  </div>
                )}
                {hasSpellingProblems(submissionResult) && (
                  <div
                    className="negative-badge"
                    title={
                      "AI detected several spelling/typo issues in your review"
                    }
                    aria-label={"Several spelling errors badge"}
                    role="img"
                  >
                    <span className="negative-symbol">✖</span>
                    <span className="negative-text">Has spelling errors</span>
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
                            {/* Explain this button */}
                            <div className="explain-row">
                              <button
                                className={`explain-button ${
                                  explainLoading[i.id] ? "disabled" : ""
                                }`}
                                disabled={!!explainLoading[i.id]}
                                onClick={async () => {
                                  if (!currentTest) return;
                                  if (explainLoading[i.id]) return;

                                  // If explanation already exists, toggle visibility only
                                  if (explanations[i.id]) {
                                    setShownExplanations((prev) => ({
                                      ...prev,
                                      [i.id]: !prev[i.id],
                                    }));
                                    return;
                                  }

                                  // Otherwise, fetch explanation from server
                                  setError(null);
                                  // clear any prior per-issue explain error and mark loading
                                  setExplainErrors((prev) => ({
                                    ...prev,
                                    [i.id]: null,
                                  }));
                                  setExplainLoading((prev) => ({
                                    ...prev,
                                    [i.id]: true,
                                  }));
                                  try {
                                    const accessToken = await acquireApiToken();
                                    const itemText = `${i.title || i.id} [${
                                      i.category
                                    }/${i.severity}] – ${i.explanation || ""}`;
                                    const resp = await fetch(
                                      `${apiConfig.webApi}tests/${currentTest.id}/explain`,
                                      {
                                        method: "POST",
                                        headers: {
                                          Authorization: `Bearer ${accessToken}`,
                                          "Content-Type": "application/json",
                                        },
                                        body: JSON.stringify({ itemText }),
                                      }
                                    );
                                    if (!resp.ok) {
                                      throw new Error(
                                        `HTTP error! status: ${resp.status}`
                                      );
                                    }
                                    const data = await resp.json();
                                    let parsed: ExplainResponse;
                                    if (!data) {
                                      parsed = { explanation: "" };
                                    } else if (typeof data === "string") {
                                      parsed = { explanation: data };
                                    } else if (typeof data === "object") {
                                      parsed = {
                                        explanation:
                                          data.explanation ||
                                          data.Explanation ||
                                          "",
                                        examples:
                                          data.examples || data.Examples || "",
                                      };
                                    } else {
                                      parsed = { explanation: String(data) };
                                    }
                                    setExplanations((prev) => ({
                                      ...prev,
                                      [i.id]: parsed,
                                    }));
                                    // clear any per-issue explain error on success
                                    setExplainErrors((prev) => ({
                                      ...prev,
                                      [i.id]: null,
                                    }));
                                    // make it visible immediately after fetch
                                    setShownExplanations((prev) => ({
                                      ...prev,
                                      [i.id]: true,
                                    }));
                                  } catch (err) {
                                    console.error("Explain error:", err);
                                    // keep a short, local UI message and surface full details in the global error area
                                    setExplainErrors((prev) => ({
                                      ...prev,
                                      [i.id]: "Error getting explanation",
                                    }));
                                    setError(
                                      err instanceof Error
                                        ? err.message
                                        : String(err)
                                    );
                                  } finally {
                                    setExplainLoading((prev) => ({
                                      ...prev,
                                      [i.id]: false,
                                    }));
                                  }
                                }}
                              >
                                {explanations[i.id] ? (
                                  shownExplanations[i.id] ? (
                                    "Hide explanation"
                                  ) : (
                                    "Show explanation"
                                  )
                                ) : explainLoading[i.id] ? (
                                  <>
                                    Explaining… <span className="spinner" />
                                  </>
                                ) : (
                                  "Explain this"
                                )}
                              </button>
                              {explainErrors[i.id] && (
                                <div className="explain-error" role="alert">
                                  {explainErrors[i.id]}
                                </div>
                              )}
                            </div>
                            {/* Keep the explanation block mounted so CSS transitions can run; toggle open/closed class */}
                            <div
                              className={`explanation-block ${
                                explanations[i.id] && shownExplanations[i.id]
                                  ? "open"
                                  : "closed"
                              }`}
                              aria-hidden={
                                !(explanations[i.id] && shownExplanations[i.id])
                              }
                            >
                              {explanations[i.id] && (
                                <>
                                  <div className="explanation-text">
                                    {explanations[i.id].explanation}
                                  </div>
                                  {explanations[i.id].examples && (
                                    <div className="explanation-examples">
                                      <strong>Examples:</strong>
                                      <pre>{explanations[i.id].examples}</pre>
                                    </div>
                                  )}
                                </>
                              )}
                              {/* proposedFix removed: UI displays explanation and examples only */}
                            </div>
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
