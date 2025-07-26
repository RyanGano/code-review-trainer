import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
  useMsal,
} from "@azure/msal-react";
import SignInButton from "./components/SignInButton";
import SignOutButton from "./components/SignOutButton";
import ProfileData from "./components/ProfileData";
import CodeReviewPractice from "./components/CodeReviewPractice";
import "./App.css";

function App() {
  const { accounts } = useMsal();

  return (
    <>
      <header className="hero">
        <h1>Code Review Trainer</h1>
        <p className="tagline">Master the art of code review in the AI era</p>
      </header>

      <main className="content">
        <section className="description">
          <p>
            In an environment where coding is increasingly done by AI models
            rather than humans, the ability to{" "}
            <strong>review code effectively</strong> has become more critical
            than ever. Code Review Trainer is your training ground for
            developing sharp, efficient code review skills.
          </p>

          <div className="features">
            <div className="feature">
              <h3>🎯 Choose Your Challenge</h3>
              <p>
                Select your skill level and preferred programming language to
                get started.
              </p>
            </div>

            <div className="feature">
              <h3>📋 Review Mock Pull Requests</h3>
              <p>
                Practice on realistic pull requests designed to test your review
                abilities.
              </p>
            </div>

            <div className="feature">
              <h3>📈 Get AI Feedback</h3>
              <p>
                Receive detailed AI-powered evaluation of your reviews with tips
                for improvement and insights you may have missed.
              </p>
            </div>
          </div>

          <div className="cta">
            <CodeReviewPractice />
          </div>
          <AuthenticatedTemplate>
            <div style={{ marginBottom: "20px" }}>
              <h2>Welcome, {accounts[0]?.name || "User"}!</h2>
              <p>
                You are successfully authenticated and can now access the
                application.
              </p>
              <SignOutButton />
            </div>
            <ProfileData />
          </AuthenticatedTemplate>

          <UnauthenticatedTemplate>
            <div
              style={{
                marginBottom: "20px",
                padding: "20px",
                border: "2px solid #ffa500",
                borderRadius: "8px",
                backgroundColor: "#fff3cd",
              }}
            >
              <h2>Authentication Required</h2>
              <p>
                You must sign in to access this application. Please authenticate
                using your Microsoft account.
              </p>
              <SignInButton />
            </div>
          </UnauthenticatedTemplate>

          <footer className="disclaimer">
            <p>
              <em>
                Note: This platform and its review evaluations are AI-generated
                to provide consistent, scalable training.
              </em>
            </p>
          </footer>
        </section>
      </main>
    </>
  );
}

export default App;
