import { useMsal } from "@azure/msal-react";

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
        <div style={{ display: 'flex', gap: '10px', alignItems: 'center' }}>
            <button onClick={() => handleLogout("popup")}>
                Sign out using Popup
            </button>
            <button onClick={() => handleLogout("redirect")}>
                Sign out using Redirect
            </button>
        </div>
    );
};

export default SignOutButton;