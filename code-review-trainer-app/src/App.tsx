import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
  useMsal,
} from "@azure/msal-react";
import SignInButton from "./components/SignInButton";
import SignOutButton from "./components/SignOutButton";
import ProfileData from "./components/ProfileData";
import CodeReviewPractice from "./components/CodeReviewPractice";
import "./App.less";

function App() {
  const { accounts } = useMsal();

  return (
    <>
      <div className="app-header">
        <div className="auth-controls">
          <UnauthenticatedTemplate>
            <SignInButton />
          </UnauthenticatedTemplate>
          
          <AuthenticatedTemplate>
            <span className="welcome-text">Welcome {accounts[0]?.name || "User"}</span>
            <SignOutButton />
          </AuthenticatedTemplate>
        </div>
      </div>

      <header className="hero">
        <h1>Code Review Trainer</h1>
        <p className="tagline">Master the art of code review in the AI era</p>
      </header>

      <main className="content">
        <section className="description">
          <UnauthenticatedTemplate>
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

            <div className="auth-warning">
              <p>Sign in to get started</p>
              <SignInButton />
            </div>
          </UnauthenticatedTemplate>

          <AuthenticatedTemplate>
            <div className="cta">
              <CodeReviewPractice />
            </div>
            <ProfileData />
          </AuthenticatedTemplate>

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
