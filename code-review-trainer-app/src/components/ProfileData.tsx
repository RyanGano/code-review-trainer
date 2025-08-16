import { useState, useEffect, useCallback } from "react";
import { useMsal } from "@azure/msal-react";
import { apiConfig } from "../authConfig";
import "./shared.less";

const ProfileData = () => {
  const { instance, accounts } = useMsal();
  const [error, setError] = useState<string | null>(null);

  const fetchUserInfo = useCallback(async () => {
    if (accounts.length === 0) {
      return;
    }

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

      // If successful, we don't need to store the user data since we're not displaying it
      await userResponse.json();
    } catch (error) {
      console.error("Error fetching user info:", error);
      setError(
        error instanceof Error ? error.message : "Unknown error occurred"
      );
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

  return (
    <div className="profile-container">
      {error && (
        <div>
          <h3>User Profile</h3>
          <div className="error-message">Error: {error}</div>
        </div>
      )}
    </div>
  );
};

export default ProfileData;
