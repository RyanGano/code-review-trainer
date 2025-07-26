import { useState, useCallback } from "react";
import { useMsal } from "@azure/msal-react";
import { apiConfig } from "../authConfig";
import ReactDiffViewer from "react-diff-viewer-continued";

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
      const testResponse = await fetch(`${apiConfig.webApi}/tests/?level=Easy`, {
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
    alert("Review submitted! (This is just a placeholder - functionality will be added later)");
  };

  const handleNewTest = () => {
    fetchCodeReviewTest();
  };

  const handleStartPracticing = () => {
    fetchCodeReviewTest();
  };

  if (!currentTest && !isLoading && !error) {
    return (
      <div style={{ textAlign: "center", padding: "20px" }}>
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
          <p style={{ color: "red", marginTop: "10px" }}>
            Please sign in to start practicing
          </p>
        )}
      </div>
    );
  }

  return (
    <div style={{ padding: "20px" }}>
      <div style={{ marginBottom: "20px", textAlign: "center" }}>
        <h2>Code Review Practice - {currentTest?.level || "Easy"} Level</h2>
        <button onClick={handleNewTest} disabled={isLoading}>
          Get New Code Sample
        </button>
      </div>

      {isLoading && (
        <div style={{ textAlign: "center", padding: "20px" }}>
          Loading code sample...
        </div>
      )}

      {error && (
        <div style={{ 
          color: "red", 
          padding: "10px", 
          border: "1px solid red", 
          borderRadius: "4px",
          marginBottom: "20px" 
        }}>
          Error: {error}
        </div>
      )}

      {currentTest && (
        <div>
          <div style={{ marginBottom: "20px" }}>
            <h3>Code to Review:</h3>
            <div style={{ 
              border: "1px solid #ccc", 
              borderRadius: "4px",
              overflow: "hidden"
            }}>
              <ReactDiffViewer
                oldValue=""
                newValue={currentTest.problem}
                splitView={false}
                showDiffOnly={false}
                hideLineNumbers={false}
                useDarkTheme={false}
                leftTitle="Original"
                rightTitle="Code for Review"
              />
            </div>
          </div>

          <div style={{ marginBottom: "20px" }}>
            <h3>Your Review Comments:</h3>
            <textarea
              value={reviewComments}
              onChange={(e) => setReviewComments(e.target.value)}
              placeholder="Enter your code review comments here. What issues do you see? What would you suggest to improve this code?"
              style={{
                width: "100%",
                minHeight: "150px",
                padding: "10px",
                border: "1px solid #ccc",
                borderRadius: "4px",
                fontFamily: "monospace",
                fontSize: "14px",
                resize: "vertical"
              }}
            />
          </div>

          <div style={{ textAlign: "center" }}>
            <button
              onClick={handleSubmitReview}
              disabled={!reviewComments.trim()}
              style={{
                padding: "10px 20px",
                backgroundColor: reviewComments.trim() ? "#007bff" : "#ccc",
                color: "white",
                border: "none",
                borderRadius: "4px",
                cursor: reviewComments.trim() ? "pointer" : "not-allowed",
                fontSize: "16px"
              }}
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