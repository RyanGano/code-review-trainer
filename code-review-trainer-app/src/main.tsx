import { StrictMode, useEffect, useState } from "react";
import { createRoot } from "react-dom/client";
import { PublicClientApplication } from "@azure/msal-browser";
import { MsalProvider } from "@azure/msal-react";
import { msalConfig } from "./authConfig";
import "./index.less";
import App from "./App.tsx";

function isCryptoAvailable(): boolean {
  try {
    return (
      typeof window !== "undefined" && !!window.crypto && !!window.crypto.subtle
    );
  } catch {
    return false;
  }
}

// eslint-disable-next-line react-refresh/only-export-components
const Root = () => {
  const [cryptoReady, setCryptoReady] = useState(false);
  const [msalInstance, setMsalInstance] =
    useState<PublicClientApplication | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isCryptoAvailable()) {
      setError(
        "This application requires the Web Crypto API (window.crypto.subtle). Please open it on a modern browser (Edge, Chrome, Firefox, Safari) using http://localhost (or HTTPS)."
      );
      return;
    }
    try {
      const instance = new PublicClientApplication(msalConfig);
      setMsalInstance(instance);
      setCryptoReady(true);
    } catch (e) {
      console.error("Failed to initialize MSAL", e);
      setError(
        "Failed to initialize authentication library. See console for details."
      );
    }
  }, []);

  if (error) {
    return (
      <div
        style={{
          fontFamily: "sans-serif",
          padding: "2rem",
          maxWidth: 700,
          margin: "0 auto",
        }}
      >
        <h1>Startup Error</h1>
        <p>{error}</p>
        <h2>Troubleshooting</h2>
        <ul>
          <li>
            Ensure you are accessing via http://localhost:5173 (not a raw IP or
            file:// URL).
          </li>
          <li>Do not use legacy/IE mode or very old browser versions.</li>
          <li>
            If using a corporate environment, confirm policies do not disable
            Web Crypto.
          </li>
          <li>Try a hard reload (Ctrl+Shift+R).</li>
        </ul>
      </div>
    );
  }

  if (!cryptoReady || !msalInstance) {
    return (
      <div style={{ padding: "2rem", fontFamily: "sans-serif" }}>
        Initializing…
      </div>
    );
  }

  return (
    <MsalProvider instance={msalInstance}>
      <App />
    </MsalProvider>
  );
};

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <Root />
  </StrictMode>
);
