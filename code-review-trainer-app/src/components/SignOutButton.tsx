import { useMsal } from "@azure/msal-react";
import "./shared.less";

const SignOutButton = () => {
    const { instance } = useMsal();

    const handleLogout = (logoutType: string) => {
        if (logoutType === "popup") {
            instance.logoutPopup({
                postLogoutRedirectUri: window.location.origin,
                mainWindowRedirectUri: window.location.origin
            });
        } else if (logoutType === "redirect") {
            instance.logoutRedirect({
                postLogoutRedirectUri: window.location.origin,
            });
        }
    }

    return (
        <div className="button-group">
            <button onClick={() => handleLogout("popup")}>
                Sign out
            </button>
        </div>
    );
};

export default SignOutButton;