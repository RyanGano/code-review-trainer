import { useMsal } from "@azure/msal-react";
import { redirectBridgeUri } from "../authConfig";
import "./shared.less";

const SignOutButton = () => {
  const { instance } = useMsal();

  const handleLogout = (logoutType: string) => {
    if (logoutType === "popup") {
      instance.logoutPopup({
        postLogoutRedirectUri: redirectBridgeUri,
        mainWindowRedirectUri: window.location.origin,
      });
    } else if (logoutType === "redirect") {
      instance.logoutRedirect({
        postLogoutRedirectUri: window.location.origin,
      });
    }
  };

  return (
    <div className="button-group">
      <button className="sign-in-link" onClick={() => handleLogout("popup")}>
        Sign out
      </button>
    </div>
  );
};

export default SignOutButton;
