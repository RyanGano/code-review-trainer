import { useMsal } from "@azure/msal-react";
import { loginRequest } from "../authConfig";
import "./shared.less";

const SignInButton = () => {
  const { instance, accounts } = useMsal();

  const handleSignIn = () => {
    instance.loginPopup(loginRequest).catch((e) => {
      console.error("Sign in failed:", e);
      // Provide a minimal user-visible message for common consent errors
      const code = (e && e.errorCode) || (e && e.message) || "";
      if (code && String(code).includes("consent")) {
        alert(
          "Sign-in completed but additional consent is required to call the API. Please contact your administrator or sign in with an account that can grant consent."
        );
      }
    });
  };

  if (accounts.length > 0) return null;

  return (
    <div className="button-group">
      <button className="sign-in-link" onClick={handleSignIn}>
        Sign in
      </button>
    </div>
  );
};

export default SignInButton;
