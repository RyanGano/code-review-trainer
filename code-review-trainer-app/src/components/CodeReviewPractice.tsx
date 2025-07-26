import { useState, useCallback } from "react";
import { useMsal } from "@azure/msal-react";
import { apiConfig } from "../authConfig";
import CodeMirror from "@uiw/react-codemirror";
import { javascript } from "@codemirror/lang-javascript";

import "./CodeReviewPractice.css";

interface CodeReviewTest {
  level: string;
  problem: string;
}

const CodeReviewPractice = () => {
  const { instance, accounts } = useMsal();
  const [currentTest, setCurrentTest] = useState<CodeReviewTest | null>(null);
  const [reviewComments, setReviewComments] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchCodeReviewTest = useCallback(async () => {
    if (accounts.length === 0) {
      setError("You must be signed in to practice code reviews");
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      // Get access token
      const response = await instance.acquireTokenSilent({
        scopes: apiConfig.b2cScopes,
        account: accounts[0],
      });

      // Call API to get an Easy test
      const testResponse = await fetch(`${apiConfig.webApi}tests/?level=Easy`, {
        headers: {
          Authorization: `Bearer ${response.accessToken}`,
        },
      });

      if (!testResponse.ok) {
        throw new Error(`HTTP error! status: ${testResponse.status}`);
      }

      const testData = await testResponse.json();
      setCurrentTest(testData);
      setReviewComments(""); // Reset comments for new test
    } catch (error) {
      console.error("Error fetching code review test:", error);
      setError(
        error instanceof Error ? error.message : "Unknown error occurred"
      );
    } finally {
      setIsLoading(false);
    }
  }, [instance, accounts]);

  const handleSubmitReview = () => {
    // For now, this button doesn't actually do anything as specified
    alert(
      "Review submitted! (This is just a placeholder - functionality will be added later)"
    );
  };

  const handleNewTest = () => {
    fetchCodeReviewTest();
  };

  const handleStartPracticing = () => {
    fetchCodeReviewTest();
  };

  if (!currentTest && !isLoading && !error) {
    return (
      <div className="practice-start">
        <h2>Ready to Practice Code Reviews?</h2>
        <p>Click the button below to get a code sample for review.</p>
        <button
          className="start-button"
          onClick={handleStartPracticing}
          disabled={accounts.length === 0}
        >
          Start Practicing
        </button>
        {accounts.length === 0 && (
          <p className="signin-required">Please sign in to start practicing</p>
        )}
      </div>
    );
  }

  return (
    <div className="code-review-practice">
      <div className="practice-header">
        <h2>Code Review Practice - {currentTest?.level || "Easy"} Level</h2>
        <button onClick={handleNewTest} disabled={isLoading}>
          Get New Code Sample
        </button>
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
                extensions={[javascript()]}
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
            />
          </div>

          <div className="submit-section">
            <button
              onClick={handleSubmitReview}
              disabled={!reviewComments.trim()}
              className={`submit-button ${
                reviewComments.trim() ? "enabled" : "disabled"
              }`}
            >
              Submit Review
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default CodeReviewPractice;
