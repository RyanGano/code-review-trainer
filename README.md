# Code Review Tester

An AI-assisted tool for improving code review skills. This tool allows people to test and improve their abilities at reviewing code written by AI and other developers.

## Architecture

This repository contains both the backend service and frontend application for the Code Review Tester tool:

- **Backend (code-review-tester-service)**: Built with C# using minimal API
- **Frontend (code-review-trainer-app)**: Built with React and TypeScript

## Technology Stack

### Backend

- **Language**: C#
- **Framework**: .NET with minimal API
- **Architecture**: RESTful API service

### Frontend

- **Language**: TypeScript
- **Framework**: React
- **Build Tool**: Modern React tooling

## Getting Started

### Prerequisites

- .NET SDK (latest LTS version)
- Node.js (latest LTS version)
- npm or yarn package manager

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/RyanGano/code-review-tester.git
   cd code-review-tester
   ```

2. Set up the backend service:

   ```bash
   cd code-review-tester-service
   dotnet restore
   dotnet build
   ```

3. Set up the frontend application:
   ```bash
   cd ../code-review-trainer-app
   yarn install
   ```

### Running the Application

#### Backend Service

```bash
cd code-review-tester-service
dotnet run
```

The API will be available at `http://localhost:5000` (or as configured).

#### Frontend Application

```bash
cd code-review-trainer-app
yarn dev [--host]
```

The application will be available at `http://localhost:3000` (or as configured).

## Testing

### Backend Tests

```bash
cd code-review-tester-service
dotnet test
```

### Frontend Tests

```bash
cd code-review-trainer-app
yarn test
```

**Note**: No tests are implemented yet. Test infrastructure will be added as the project develops.

## Contributing

We welcome contributions to the Code Review Tester project! Here's how you can help:

### Getting Started with Contributing

1. **Fork the repository** on GitHub
2. **Create a feature branch** from `main`:
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **Make your changes** following the coding standards below
4. **Test your changes** thoroughly
5. **Commit your changes** with clear, descriptive commit messages
6. **Push to your fork** and submit a pull request

### Coding Standards

#### Backend (C#)

- Follow standard C# naming conventions
- Use meaningful variable and method names
- Write XML documentation for public APIs
- Follow [SOLID principles](https://en.wikipedia.org/wiki/SOLID)
- Use dependency injection where appropriate

#### Frontend (React/TypeScript)

- Use TypeScript for all new code
- Follow React best practices and hooks patterns
- Use meaningful component and variable names
- Write JSDoc comments for complex functions
- Follow accessibility (a11y) guidelines

### Pull Request Guidelines

- **Title**: Use a clear, descriptive title
- **Description**: Explain what your changes do and why
- **Testing**: Describe how you tested your changes
- **Documentation**: Update documentation if needed
- **Breaking Changes**: Clearly mark any breaking changes

### Code Review Process

1. All submissions require review from at least one maintainer
2. We may suggest changes, improvements, or alternatives
3. Once approved, a maintainer will merge your pull request

### Reporting Issues

- Use the GitHub issue tracker to report bugs
- Include as much detail as possible
- Provide steps to reproduce the issue
- Include environment information (OS, browser, etc.)

### Development Setup

For detailed development setup instructions, see the installation section above. Make sure to:

- Install all dependencies
- Run the application locally to verify everything works
- Follow the coding standards for your contributions

## License

This project is open source. License details will be added as the project evolves.

## Contact

For questions or suggestions, please open an issue on GitHub.
