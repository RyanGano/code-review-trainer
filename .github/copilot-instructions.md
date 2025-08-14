# Code Review Trainer

Code Review Trainer is an AI-assisted tool for improving code review skills. This is a full-stack application with a C# .NET backend API and a React TypeScript frontend that allows users to practice reviewing code samples at different difficulty levels.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Prerequisites and Installation
- Install .NET SDK 8.0 or later: `dotnet --version` should show 8.x
- Install Node.js 18+ (frontend workflow uses 22.12.0): `node --version` should show 18.x or higher
- Yarn package manager is required: `yarn --version` should show 1.22.x

### Bootstrap and Build Process
Follow these exact steps to set up the development environment:

1. **Clone and navigate to repository:**
   ```bash
   git clone https://github.com/RyanGano/code-review-trainer.git
   cd code-review-trainer
   ```

2. **Backend setup (code-review-trainer-service):**
   ```bash
   cd code-review-trainer-service
   dotnet restore
   ```
   - Takes ~35 seconds. NEVER CANCEL. Set timeout to 2+ minutes.

   ```bash
   dotnet build
   ```
   - Takes ~14 seconds. NEVER CANCEL. Set timeout to 2+ minutes.

3. **Frontend setup (code-review-trainer-app):**
   ```bash
   cd ../code-review-trainer-app
   yarn install
   ```
   - Takes ~47 seconds. NEVER CANCEL. Set timeout to 3+ minutes.
   - Will show warnings about unmet peer dependencies - this is normal and expected.

   ```bash
   yarn build
   ```
   - Takes ~5 seconds. Produces production build in `dist/` directory.

### Running the Application

#### Backend Service
```bash
cd code-review-trainer-service
dotnet run
```
- Starts immediately (~2 seconds)
- Runs on `http://localhost:5137` (note: port 5137, NOT 5000 as mentioned in README)
- Root endpoint `/` returns "I'm ALIVE!" for health check
- Protected endpoints require Microsoft Entra ID authentication

#### Frontend Application
```bash
cd code-review-trainer-app
yarn dev --host
```
- Starts in ~1 second
- Runs on `http://localhost:5173`
- Hot reload enabled for development
- ALWAYS use `--host` flag to enable network access

### Testing Commands
```bash
# Backend tests (no tests implemented yet - returns immediately ~1 second)
cd code-review-trainer-service
dotnet test
```
- Returns: "Determining projects to restore... All projects are up-to-date for restore."
- Takes ~1 second. No actual tests run yet.

```bash
# Frontend tests (not implemented - command will fail)
cd code-review-trainer-app
yarn test  # Will show "error Command 'test' not found" - this is expected
```
- Expected output: "yarn run v1.22.22 error Command 'test' not found."
- This is normal - no test script configured in package.json yet.

### Linting and Code Quality
```bash
# Frontend linting (ALWAYS run before committing)
cd code-review-trainer-app
yarn lint
```
- Takes ~1 second
- Must pass or CI will fail
- ESLint configuration uses TypeScript rules and React best practices

## Validation Scenarios

### CRITICAL: Manual Application Testing
After making any changes, ALWAYS test the complete user workflow:

1. **Start both services** (backend on 5137, frontend on 5173)
2. **Navigate to `http://localhost:5173`** in browser
3. **Verify authentication UI displays** with "Sign in using Popup" and "Sign in using Redirect" buttons
4. **Test difficulty selection dropdown** (Easy/Medium options)
5. **Verify "Start Practicing" button is disabled** until authentication
6. **Check console for API Base URL log** (should show the configured API endpoint)

Expected behavior:
- Page title: "Code Review Trainer - Improve Your Code Review Skills"
- Authentication required message displayed
- Difficulty selection dropdown functional
- No critical JavaScript errors in browser console

### API Endpoint Testing
```bash
# Test health endpoint (should return "I'm ALIVE!")
curl http://localhost:5137/

# Test protected endpoint (should return 401 without auth)
curl -w "Status: %{http_code}" http://localhost:5137/user
```

## CI/CD Pipeline Information

### Backend Pipeline (.github/workflows/publish-backend.yml)
- Runs on Windows (windows-latest)
- Uses .NET 8.x
- Build timeout: Set to 10+ minutes for safety
- Deploys to Azure Web App

### Frontend Pipeline (.github/workflows/publish-frontend.yml)
- Runs on Ubuntu (ubuntu-latest)  
- Uses Node.js 22.12.0
- Build timeout: Set to 10+ minutes for safety
- Deploys to Azure Static Web Apps
- Requires environment variables: VITE_AZURE_TENANT_ID, VITE_AZURE_CLIENT_ID, VITE_API_BASE_URL

## Key Projects and Architecture

### Backend (code-review-trainer-service)
- **Technology**: C# .NET 8.0 minimal API
- **Authentication**: Microsoft Entra ID with JWT Bearer tokens
- **Key Files**:
  - `Program.cs` - Main API configuration and endpoints
  - `CodeReviewProblems/EasyCodeReviewProblems.cs` - Easy difficulty problems
  - `CodeReviewProblems/MediumCodeReviewProblems.cs` - Medium difficulty problems
  - `appsettings.json` - Configuration with Azure Key Vault integration
- **Endpoints**:
  - `GET /` - Health check (public)
  - `GET /user` - User info (requires auth)
  - `GET /tests/?level={Easy|Medium}` - Get code review problems (requires auth)

### Frontend (code-review-trainer-app)
- **Technology**: React 19.x with TypeScript, Vite build tool
- **Authentication**: Microsoft Authentication Library (MSAL) for Azure AD
- **Key Files**:
  - `src/components/CodeReviewPractice.tsx` - Main practice interface
  - `src/authConfig.ts` - MSAL configuration
  - `eslint.config.js` - Linting rules
  - `.env.example` - Environment variables template
- **Dependencies**: CodeMirror for code display, MSAL for authentication

### Configuration Files
- `.github/workflows/` - CI/CD pipelines
- `code-review-trainer.sln` - Visual Studio solution file
- `.vscode/` - VS Code debugging configuration

## Common Commands Reference

### Repository Structure
```
code-review-trainer/
├── .github/workflows/           # CI/CD pipelines
├── .vscode/                    # VS Code configuration
├── code-review-trainer-service/ # Backend C# API
│   ├── CodeReviewProblems/     # Problem definitions
│   ├── Program.cs             # Main API entry point
│   └── *.csproj              # .NET project file
├── code-review-trainer-app/    # Frontend React app
│   ├── src/                   # Source code
│   ├── package.json          # Node.js dependencies
│   └── vite.config.ts        # Build configuration
└── README.md                 # Basic setup instructions
```

### Environment Variables Required (Frontend)
- `VITE_AZURE_TENANT_ID` - Microsoft Entra ID tenant
- `VITE_AZURE_CLIENT_ID` - Application registration ID  
- `VITE_API_BASE_URL` - Backend API URL (defaults to http://localhost:5137)

## Development Tips

### Always Run Before Committing
```bash
cd code-review-trainer-app
yarn lint  # Must pass or CI fails
yarn build # Verify production build works
```

### Debugging
- Backend runs in Development mode by default with detailed logging
- Frontend has hot reload - changes appear immediately
- Use browser dev tools to inspect network requests to API
- Check browser console for authentication status logs

### Common Issues
- **Port conflicts**: Backend uses 5137, frontend uses 5173
- **Authentication**: Protected endpoints return 401 without valid Azure AD token
- **CORS**: Backend configured for specific frontend origins
- **Missing dependencies**: Run `yarn install` if frontend fails to start
- **Build warnings**: Large chunk size warning in frontend build is expected