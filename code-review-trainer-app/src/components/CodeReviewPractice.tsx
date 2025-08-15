import { useState, useCallback } from "react";
import { useMsal } from "@azure/msal-react";
import { apiConfig } from "../authConfig";
import CodeMirror from "@uiw/react-codemirror";
import { csharp } from "@replit/codemirror-lang-csharp";

import "./CodeReviewPractice.less";

interface CodeReviewTest {
  level: string;
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
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submissionResult, setSubmissionResult] = useState<string | null>(null);

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

      // Call API to get a test with the selected difficulty
      const testResponse = await fetch(`${apiConfig.webApi}tests/?level=${selectedDifficulty}`, {
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
      setSubmissionResult(null); // Reset previous submission results
    } catch (error) {
      console.error("Error fetching code review test:", error);
      setError(
        error instanceof Error ? error.message : "Unknown error occurred"
      );
    } finally {
      setIsLoading(false);
    }
  }, [instance, accounts, selectedDifficulty]);

  const handleSubmitReview = async () => {
    if (!currentTest || !reviewComments.trim() || accounts.length === 0) {
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      // Get access token
      const response = await instance.acquireTokenSilent({
        scopes: apiConfig.b2cScopes,
        account: accounts[0],
      });

      // Submit review to backend
      const submitResponse = await fetch(`${apiConfig.webApi}tests/${currentTest.id}`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${response.accessToken}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ review: reviewComments.trim() }),
      });

      if (!submitResponse.ok) {
        throw new Error(`HTTP error! status: ${submitResponse.status}`);
      }

      const result = await submitResponse.json();
      setSubmissionResult(result.message);
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

  const handleStartPracticing = () => {
    fetchCodeReviewTest();
  };

  if (!currentTest && !isLoading && !error) {
    return (
      <div className="practice-start">
        <h2>Ready to Practice Code Reviews?</h2>
        <p>Choose your difficulty level and click the button below to get a code sample for review.</p>
        
        <div className="difficulty-selection">
          <label htmlFor="difficulty-select">Difficulty Level:</label>
          <select 
            id="difficulty-select"
            value={selectedDifficulty} 
            onChange={(e) => setSelectedDifficulty(e.target.value)}
            className="difficulty-dropdown"
          >
            <option value="Easy">Easy</option>
            <option value="Medium">Medium</option>
          </select>
        </div>
        
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
        <h2>Code Review Practice - {currentTest?.level || selectedDifficulty} Level</h2>
        <div className="header-controls">
          <div className="difficulty-selection">
            <label htmlFor="difficulty-select-active">Difficulty:</label>
            <select 
              id="difficulty-select-active"
              value={selectedDifficulty} 
              onChange={(e) => setSelectedDifficulty(e.target.value)}
              className="difficulty-dropdown"
            >
              <option value="Easy">Easy</option>
              <option value="Medium">Medium</option>
            </select>
          </div>
          <button onClick={handleNewTest} disabled={isLoading}>
            Get New Code Sample
          </button>
        </div>
        {currentTest && (
          <div className="test-info">
            <span className="test-id">Test ID: {currentTest.id}</span>
          </div>
        )}
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
                extensions={[csharp()]}
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
              disabled={!reviewComments.trim() || isSubmitting}
              className={`submit-button ${
                reviewComments.trim() && !isSubmitting ? "enabled" : "disabled"
              }`}
            >
              {isSubmitting ? "Submitting…" : "Submit Review"}
            </button>
            {isSubmitting && <div className="loading-indicator">Please wait while we evaluate your review…</div>}
          </div>

          {submissionResult && (
            <div className="submission-result">
              <h4>Review Evaluation:</h4>
              <p>{submissionResult}</p>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default CodeReviewPractice;
