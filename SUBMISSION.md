# DevGuardian AI – Hackathon Submission

---

## Project Title
**DevGuardian AI – Spec-Driven Autonomous Incident Response**

---

## Demo Video
> Upload to YouTube / Vimeo and replace the link below.

🎬 **Demo Video:** `https://youtu.be/YOUR_VIDEO_ID`
*(2-minute walkthrough: paste logs → pipeline runs → incident detected → fix generated → deploy plan produced)*

---

## GitHub Repository
🔗 **Public Repo:** `https://github.com/sandyArt/DevGuardian-AI`

---

## Problem Statement

Production incidents cost engineering teams an average of **$5,600 per minute** (Gartner).
When an outage hits at 2 AM, on-call engineers must:

1. Triage alerts across dozens of services
2. Manually correlate logs to identify the root cause
3. Write and review a code fix under pressure
4. Coordinate a safe rollout

DevGuardian AI compresses this **2–4 hour cycle into under 30 seconds** using a
fully autonomous, spec-driven multi-agent pipeline powered by Microsoft Semantic Kernel
and Azure OpenAI.

---

## Solution Overview

DevGuardian AI is an **autonomous incident response platform** where every AI agent's
behaviour is defined by a YAML specification — not hardcoded logic. This means:

- **Zero code changes** to add or modify an agent — just edit a YAML file
- **MCP-compatible** tool endpoints that any AI host or agent can discover and call
- **Auditable** — every agent has a named spec that can be version-controlled and reviewed
- **Extensible** — drop in a SecurityAgent or PerformanceAgent without touching C#

### 4-Stage Autonomous Pipeline

```
Raw Logs → [MonitoringAgent] → [RootCauseAgent] → [FixAgent] → [DeployAgent]
```

| Stage | Agent | Output |
|-------|-------|--------|
| 1 | MonitoringAgent | JSON: service, severity, error rate, time range |
| 2 | RootCauseAgent | JSON: file, line, method, cause, confidence |
| 3 | FixAgent | C# BEFORE/AFTER patch + unit test |
| 4 | DeployAgent | JSON: pre-deploy checklist, steps, KPIs, rollback |

---

## Features

| Feature | Details |
|---------|---------|
| **Spec-driven agents** | YAML defines name, prompt, inputs, max_tokens, tools |
| **Agent runtime** | Loads and executes any spec dynamically via Semantic Kernel |
| **MCP tool layer** | `/mcp/tools` – discoverable by any MCP-compatible host |
| **Diagnostic plugin** | Extracts errors, calculates error rate, isolates stack traces |
| **GitHub plugin** | Creates PRs with the generated fix (Octokit stub, real-ready) |
| **Live dashboard** | Single-page UI – paste logs, watch the pipeline animate, export results |
| **Swagger UI** | Full OpenAPI docs at `/swagger` |
| **Docker support** | Multi-stage Dockerfile + docker-compose for one-command deploy |
| **CI/CD** | GitHub Actions: build → test → Docker build on every push |
| **Unit tests** | xUnit tests for SpecLoader, DiagnosticsTool, AgentSpec parsing |

---

## Technologies Used

### Hero Technologies

| Technology | How DevGuardian uses it |
|------------|------------------------|
| **Microsoft Semantic Kernel** | Agent execution engine – `InvokePromptAsync` with `ToolCallBehavior.AutoInvokeKernelFunctions` |
| **Azure OpenAI (gpt-4o)** | Powers all four agents via `AddAzureOpenAIChatCompletion` |
| **Model Context Protocol (MCP)** | `/mcp/tools` endpoints expose each agent as a discoverable, invokable MCP tool |
| **GitHub Copilot** | Used throughout development for code completion and refactoring |
| **Azure App Service / Container Apps** | Target deployment platform (Dockerfile + compose ready) |

### Supporting Technologies

- **C# / .NET 8** – Core runtime platform
- **ASP.NET Core** – Web API host with middleware pipeline
- **YamlDotNet** – Parses agent specification files
- **Octokit** – GitHub PR creation (stub → production-ready)
- **xUnit + Moq** – Unit test framework
- **Docker** – Containerised deployment
- **GitHub Actions** – CI/CD pipeline
- **Swagger / OpenAPI** – API documentation and testing

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     DevGuardian AI                           │
│                                                              │
│  ┌──────────────────┐          ┌────────────────────────┐   │
│  │  Static Web UI   │          │   ASP.NET Core API     │   │
│  │  (index.html)    │◄────────►│   /api/agents/run      │   │
│  │  Live pipeline   │          │   /api/agents/invoke   │   │
│  │  dashboard       │          │   /api/agents/catalogue│   │
│  └──────────────────┘          │   /mcp/tools/*         │   │
│                                └──────────┬─────────────┘   │
│                                           │                  │
│                                ┌──────────▼─────────────┐   │
│                                │   WorkflowEngine        │   │
│                                │   4-stage orchestrator  │   │
│                                └──────────┬─────────────┘   │
│                                           │                  │
│                     ┌─────────────────────▼──────────────┐  │
│                     │          SpecLoader                  │  │
│                     │    YAML → AgentSpec (cached)         │  │
│                     └──┬──────────┬──────────┬──────────┬─┘  │
│                        │          │          │          │     │
│              ┌─────────▼─┐  ┌────▼──────┐ ┌▼──────┐ ┌▼───┐ │
│              │ Monitoring│  │ RootCause │ │  Fix  │ │Deploy│ │
│              │   .yaml   │  │  .yaml    │ │ .yaml │ │.yaml │ │
│              └─────────┬─┘  └────┬──────┘ └┬──────┘ └┬───┘ │
│                        └────────►│          │          │     │
│                                  └──────────┴──────────┘     │
│                                             │                 │
│                                  ┌──────────▼─────────────┐  │
│                                  │      AgentRuntime       │  │
│                                  │  Semantic Kernel        │  │
│                                  │  InvokePromptAsync      │  │
│                                  └──────────┬──────────────┘  │
│                                             │                 │
│                          ┌──────────────────┼──────────────┐  │
│                          │                  │              │  │
│               ┌──────────▼──┐    ┌──────────▼──┐          │  │
│               │ Diagnostics │    │  GitHub Tool│          │  │
│               │   Plugin    │    │   Plugin    │          │  │
│               │ (SK native) │    │ (SK native) │          │  │
│               └─────────────┘    └─────────────┘          │  │
│                                             │              │  │
│                                  ┌──────────▼──────────┐  │  │
│                                  │  Azure OpenAI        │  │  │
│                                  │  gpt-4o deployment   │  │  │
│                                  └─────────────────────┘  │  │
└─────────────────────────────────────────────────────────────┘
```

#### MCP Integration Detail

```
Any MCP Host (GitHub Copilot / other AI Agent)
        │
        ▼  GET /mcp/tools
        │  Returns: JSON tool manifest with inputSchema per agent
        │
        ▼  POST /mcp/tools/monitor
           POST /mcp/tools/rootcause
           POST /mcp/tools/fix
           POST /mcp/tools/deploy
        │
        ▼  McpController → SpecLoader → AgentRuntime → Azure OpenAI
```

---
#
