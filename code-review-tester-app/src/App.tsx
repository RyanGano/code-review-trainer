import './App.css'

function App() {
  return (
    <>
      <header className="hero">
        <h1>Code Review Tester</h1>
        <p className="tagline">Master the art of code review in the AI era</p>
      </header>
      
      <main className="content">
        <section className="description">
          <p>
            In an environment where coding is increasingly done by AI models rather than humans, 
            the ability to <strong>review code effectively</strong> has become more critical than ever. 
            Code Review Tester is your training ground for developing sharp, efficient code review skills.
          </p>
          
          <div className="features">
            <div className="feature">
              <h3>🎯 Choose Your Challenge</h3>
              <p>Select your skill level and preferred programming language to get started.</p>
            </div>
            
            <div className="feature">
              <h3>📋 Review Mock Pull Requests</h3>
              <p>Practice on realistic pull requests designed to test your review abilities.</p>
            </div>
            
            <div className="feature">
              <h3>📈 Get AI Feedback</h3>
              <p>Receive detailed AI-powered evaluation of your reviews with tips for improvement and insights you may have missed.</p>
            </div>
          </div>
          
          <div className="cta">
            <button className="start-button">Start Practicing</button>
          </div>
          
          <footer className="disclaimer">
            <p><em>Note: This platform and its review evaluations are AI-generated to provide consistent, scalable training.</em></p>
          </footer>
        </section>
      </main>
    </>
  )
}

export default App
