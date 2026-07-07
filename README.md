# LINTelligent

_AI-integrated backend service for code linting (ie. analyzing code for issues). Accepts code snippets via a REST API, analyzes them using LLM, and returns a structured review identifying issues by severity, line number, type, and a short human-readable explanation for each issue._


![CI Pipeline Building and Testing](https://github.com/mazenaly256/LINTelligent/actions/workflows/ci-pipeline.yml/badge.svg)

---

## Overview

A code snippet is submitted in a linting/reviewing request, either as a direct snippet in the request body or via a public GitHub file URL.

Once the linting request reaches the system, _LINTelligent_ saves its details in the database and enqueues a background job to the job queue for handling this request, then immediately returns 202 Accepted. If a GitHub URL was provided, the code is first fetched, then the review job runs, otherwise the review job runs directly on the submitted code snippet.

After that, a worker picks up the job from the job queue and processes it, it fetches the code from the GitHub URL (if exists) then it calls the configured LLM provider with the request details and receive the review report of issues, then updates the review record with the report of linting.

Once the process of linting is finished, the service notifies the client _by sending review to their registered webhook URL (if exists)_.

### Basic system architecture diagram
Note: this diagram is a high-level overview of the core review pipeline, not a full detailed flow. Auxiliary steps (like fetching code from an external source) are omitted for clarity, and data flow is represented directly between source and destination.

![System Architecture Diagram](docs/system-architecture-diagram.png)

---

## Getting Started
- If you want to get notified once the code linting is finished, you can create a _webhook URL_ online and include it in the request body
- Go to [Swagger Interface](https://lintelligent-production.up.railway.app/swagger/index.html) to test the API and try the functionality. Use a public GitHub raw file URL (`https://raw.githubusercontent.com/{owner}/{repo}/{branch}/{path-to-file}`) or a direct code snippet in the request body.

--- 

## Tech Stack
- ASP.NET Core Web API
- xUnit & Moq
- Hangfire
- Ollama Cloud
- PostgreSQL & Entity Framework Core
- GitHub Actions

