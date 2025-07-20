import { useMsal } from "@azure/msal-react";
import { loginRequest } from "../authConfig";

const SignInButton = () => {
    const { instance } = useMsal();

    const handleLogin = (loginType: string) => {
        if (loginType === "popup") {
            instance.loginPopup(loginRequest).catch(e => {
                console.log(e);
            });
        } else if (loginType === "redirect") {
            instance.loginRedirect(loginRequest).catch(e => {
                console.log(e);
            });
        }
    }

    return (
        <div style={{ display: 'flex', gap: '10px', alignItems: 'center' }}>
            <button onClick={() => handleLogin("popup")}>
                Sign in using Popup
            </button>
            <button onClick={() => handleLogin("redirect")}>
                Sign in using Redirect
            </button>
        </div>
    );
};

export default SignInButton;