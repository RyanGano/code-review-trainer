import { useMsal } from "@azure/msal-react";
import { loginRequest } from "../authConfig";
import "./shared.less";

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
        <div className="button-group">
            <button onClick={() => handleLogin("popup")}>
                Sign in
            </button>
        </div>
    );
};

export default SignInButton;