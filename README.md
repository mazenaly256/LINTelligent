# LINTelligent

_AI-integrated backend service for code linting (i.e. analyzing code for issues), with an accompanying **MCP server** for AI-agent integration._

![CI Pipeline Building and Testing](https://github.com/mazenaly256/LINTelligent/actions/workflows/ci-pipeline.yml/badge.svg)

---

## Overview

A code snippet is submitted in a linting/reviewing request, either as a direct snippet in the request body or via a public GitHub file URL.

Once the linting request reaches the system, _LINTelligent_ saves its details in the database and enqueues a background job to the job queue for handling this request, then immediately returns 202 Accepted. If a GitHub URL was provided, the code is first fetched, then the review job runs, otherwise the review job runs directly on the submitted code snippet.

After that, a worker picks up the job from the job queue and processes it, it fetches the code from the GitHub URL (if exists) then it calls the configured LLM provider with the request details and receive the review report of issues, then updates the review record with the report of linting (JSON list of issues).

Once the process of linting is finished, the service notifies the client _by sending review to their registered webhook URL (if exists)_.

### Basic system architecture diagram
Note: this diagram is a high-level overview of the core review pipeline, not a full detailed flow. Auxiliary steps (like fetching code from an external source) are omitted for clarity, and data flow is represented directly between source and destination.

![System Architecture Diagram](LINTelligent.Api/docs/system-architecture-diagram.png)

---

## Getting Started with the RESTful API on Swagger
- If you want to get notified once the code linting is finished, you can create a _webhook URL_ ([from this link](https://webhook.site/)) and include it in the request body
- Go to [Swagger Interface](https://lintelligent-production.up.railway.app/swagger/index.html) to test the API and try the functionality. Insert a direct code snippet in the request body or a public GitHub raw file URL (can get it by opening the required file to be reviewed on a GitHub repository and click the `Raw` button then copy the URL of the page).

---

## Using LINTelligent as an MCP Server (with Claude Desktop as the AI Agent)

**LINTelligent.McpServer** exposes the service API endpoints as **callable tools** for any MCP-compatible AI agent, enabling agents to autonomously submit code for review and poll for results as part of their own reasoning, not through a separate UI.

**Tools exposed:**
- `SubmitReviewRequest` — submits a code snippet or GitHub raw file URL for review, returns a `reviewId`
- `GetReviewDetails` — retrieves a review's result by `reviewId`, call repeatedly while `Pending`/`Processing` till the review is finished

### Running MCP Server via Docker

> **Note:** Verified with Claude Desktop over stdio transport through JSON-RPC protocol. Other MCP-compatible AI agents (clients) should work the same way with the same protocol, but haven't been tested. Steps should be mostly similar, you can do your own research about integrating local MCP server with your AI agent.

The MCP server image is published publicly on GHCR, no need to build the image from the Dockerfile locally.

Firstly, pull the image:
```bash
docker pull ghcr.io/mazenaly256/lintelligent-mcp-server:latest
```


Then add to the AI agent (client) config JSON file:
```json
"mcpServers": {
  "lintelligent": {
    "command": "docker",
    "args": ["run", "-i", "--rm", "ghcr.io/mazenaly256/lintelligent-mcp-server:latest"]
  }
}
```

After configuring, fully restart your MCP client (ensure this from task manager, you should never see Claude as a background process). It will start the MCP server, perform the MCP `initialize` handshake, and discover both tools before you start chatting.
Then you can ask the AI agent for a code review, and it will call LINTelligent.

---

## Tech Stack

- ASP.NET Core Web API
- Model Context Protocol (MCP) C# SDK
- Ollama Cloud API
- Docker
- Hangfire
- GitHub Actions
- PostgreSQL & Entity Framework Core
- xUnit & Moq
