import { useState, useEffect, useCallback } from "react";
import { useMsal } from "@azure/msal-react";
import { InteractionRequiredAuthError } from "@azure/msal-browser";
import { apiConfig } from "../authConfig";
import "./shared.less";

const ProfileData = () => {
  const { instance, accounts } = useMsal();
  const [error, setError] = useState<string | null>(null);
  const [checked, setChecked] = useState(false);
  const [pendingInteraction, setPendingInteraction] = useState(false);

  const attemptFetch = useCallback(
    async (allowInteraction: boolean) => {
      if (accounts.length === 0) return;
      try {
        const request = {
          scopes: apiConfig.b2cScopes,
          account: accounts[0],
        } as const;
        const tokenResult = await instance
          .acquireTokenSilent(request)
          .catch(async (err) => {
            const e = err as { errorCode?: string };
            const needs =
              err instanceof InteractionRequiredAuthError ||
              [
                "consent_required",
                "interaction_required",
                "login_required",
              ].includes(e?.errorCode || "");
            if (!needs || !allowInteraction) {
              if (needs && !allowInteraction) setPendingInteraction(true);
              throw err;
            }
            const popup = await instance.acquireTokenPopup({
              ...request,
              prompt: "select_account",
            });
            return popup;
          });
        const accessToken = tokenResult.accessToken;
        const resp = await fetch(`${apiConfig.webApi}user`, {
          headers: { Authorization: `Bearer ${accessToken}` },
        });
        if (!resp.ok) throw new Error(`HTTP ${resp.status}`);
        setError(null);
        setPendingInteraction(false);
      } catch (err: unknown) {
        setError((err as { message?: string }).message || "Request failed");
      } finally {
        setChecked(true);
      }
    },
    [accounts, instance]
  );

  useEffect(() => {
    if (accounts.length > 0 && !checked) attemptFetch(false);
  }, [accounts, checked, attemptFetch]);

  if (accounts.length === 0) return null;

  if (!error && checked && !pendingInteraction) return null;

  return (
    <div className="profile-container">
      {error && <div className="error-message">{error}</div>}
      {pendingInteraction && (
        <button onClick={() => attemptFetch(true)}>Authorize access</button>
      )}
      {error && !pendingInteraction && (
        <button onClick={() => attemptFetch(true)}>Retry</button>
      )}
    </div>
  );
};

export default ProfileData;
