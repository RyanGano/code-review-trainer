import { useMsal } from "@azure/msal-react";
import { loginRequest } from "../authConfig";
import "./shared.less";

const SignInButton = () => {
  const { instance, accounts } = useMsal();

  const handleSignIn = () => {
    instance.loginPopup(loginRequest).catch(() => {
      /* optionally surface error to user */
    });
  };

  if (accounts.length > 0) return null;

  return (
    <div className="button-group">
      <button onClick={handleSignIn}>Sign in</button>
    </div>
  );
};

export default SignInButton;
