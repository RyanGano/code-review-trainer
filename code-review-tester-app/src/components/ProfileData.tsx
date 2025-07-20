import { useState, useEffect, useCallback } from "react";
import { useMsal } from "@azure/msal-react";
import { apiConfig } from "../authConfig";

interface UserInfo {
  name?: string;
  isAuthenticated: boolean;
  claims: Array<{ type: string; value: string }>;
}

const ProfileData = () => {
  const { instance, accounts } = useMsal();
  const [userInfo, setUserInfo] = useState<UserInfo | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchUserInfo = useCallback(async () => {
    if (accounts.length === 0) {
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

      // Call API
      const userResponse = await fetch(`${apiConfig.webApi}user`, {
        headers: {
          Authorization: `Bearer ${response.accessToken}`,
        },
      });

      if (!userResponse.ok) {
        throw new Error(`HTTP error! status: ${userResponse.status}`);
      }

      const userData = await userResponse.json();
      setUserInfo(userData);
    } catch (error) {
      console.error("Error fetching user info:", error);
      setError(
        error instanceof Error ? error.message : "Unknown error occurred"
      );
    } finally {
      setIsLoading(false);
    }
  }, [instance, accounts]);

  useEffect(() => {
    if (accounts.length > 0) {
      fetchUserInfo();
    }
  }, [accounts, fetchUserInfo]);

  if (accounts.length === 0) {
    return <div>No user signed in</div>;
  }

  const account = accounts[0];

  return (
    <div
      style={{
        padding: "20px",
        border: "1px solid #ccc",
        borderRadius: "8px",
        margin: "10px 0",
      }}
    >
      <h3>User Profile</h3>
      <div style={{ marginBottom: "15px" }}>
        <strong>Display Name:</strong> {account.name || "N/A"}
      </div>
      <div style={{ marginBottom: "15px" }}>
        <strong>Username:</strong> {account.username || "N/A"}
      </div>

      <h4>API User Information</h4>
      {isLoading && <div>Loading user info from API...</div>}
      {error && <div style={{ color: "red" }}>Error: {error}</div>}
      {userInfo && (
        <div>
          <div>
            <strong>Name:</strong> {userInfo.name || "N/A"}
          </div>
          <div>
            <strong>Authenticated:</strong>{" "}
            {userInfo.isAuthenticated ? "Yes" : "No"}
          </div>
          <div>
            <strong>Claims:</strong>
            <ul style={{ maxHeight: "200px", overflowY: "auto" }}>
              {userInfo.claims.map((claim, index) => (
                <li key={index}>
                  <strong>{claim.type}:</strong> {claim.value}
                </li>
              ))}
            </ul>
          </div>
        </div>
      )}

      <button onClick={fetchUserInfo} disabled={isLoading}>
        Refresh User Info
      </button>
    </div>
  );
};

export default ProfileData;
