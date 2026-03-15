# DevGuardian AI

> **Spec-Driven Multi-Agent AI for Automated Incident Response**
> Built on C# · .NET 8 · Microsoft Semantic Kernel · Azure OpenAI · MCP

---

## Architecture

```
POST /api/agents/run
        │
        ▼
  WorkflowEngine
        │
   ┌────┴────────────────────────────────────┐
   │           SpecLoader (YAML)              │
   └────┬────────────────────────────────────┘
        │
  ┌─────▼──────┐  ┌───────────────┐  ┌──────────┐  ┌────────────┐
  │ Monitoring │→ │  RootCause    │→ │   Fix    │→ │   Deploy   │
  │   Agent    │  │    Agent      │  │  Agent   │  │   Agent    │
  └────────────┘  └───────────────┘  └──────────┘  └────────────┘
        │                                   │
        └── Diagnostics Plugin   ── GitHub Plugin
```

**Key Design Principles**

| Principle | How DevGuardian implements it |
|-----------|------------------------------|
| Spec-driven | Agent behaviour defined in YAML – no code changes needed to add agents |
| MCP-compatible | `/mcp/tools/*` endpoints expose every agent as a discoverable tool |
| Composable | Each agent's output is the next agent's input (chain-of-thought pipeline) |
| Observable | Structured JSON output from every agent stage |

---

## Project Structure

```
DevGuardianAI/
├── DevGuardianAI.sln
├── specs/                          ← YAML agent specifications
│   ├── monitoring-agent.yaml
│   ├── rootcause-agent.yaml
│   ├── fix-agent.yaml
│   └── deploy-agent.yaml
└── src/
    ├── DevGuardian.API/            ← ASP.NET Core host + Swagger + MCP endpoints
    │   ├── Controllers/
    │   │   ├── AgentsController.cs
    │   │   └── McpController.cs
    │   ├── Models/ApiModels.cs
    │   ├── Program.cs
    │   └── appsettings.json
    ├── DevGuardian.AgentRuntime/   ← Core spec-execution engine
    │   ├── Models/
    │   │   ├── AgentSpec.cs
    │   │   └── AgentTool.cs
    │   ├── AgentRuntime.cs
    │   ├── KernelFactory.cs
    │   ├── SpecLoader.cs
    │   └── WorkflowEngine.cs
    └── DevGuardian.Tools/          ← Semantic Kernel plugins
        ├── DiagnosticsTool.cs
        ├── GitHubTool.cs
        └── PluginRegistrar.cs
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Azure OpenAI resource with a **gpt-4o** deployment
- (Optional) GitHub Personal Access Token for PR creation

---

## Quick Start

### 1 – Configure credentials

Edit `src/DevGuardian.API/appsettings.Development.json`:

```json
{
  "AzureOpenAI": {
    "Endpoint":   "https://<your-resource>.openai.azure.com/",
    "Key":        "<your-api-key>",
    "Deployment": "gpt-4o"
  },
  "GitHub": {
    "Token": "<your-github-pat>",
    "Owner": "your-org",
    "Repo":  "your-repo"
  }
}
```

Or use environment variables:

```bash
set AZURE_OPENAI_ENDPOINT=https://...
set AZURE_OPENAI_KEY=...
set AZURE_OPENAI_DEPLOYMENT=gpt-4o
```

### 2 – Restore & run

```bash
cd DevGuardianAI
dotnet restore
dotnet run --project src/DevGuardian.API
```

Swagger UI is available at **http://localhost:5000**.

---

## API Reference

### Full pipeline

```http
POST /api/agents/run
Content-Type: application/json

{
  "logs": "2024-03-12T10:05:01 ERROR PaymentService.cs line 210 NullReferenceException..."
}
```

**Response**

```json
{
  "incident":   "{ ... monitoring agent JSON ... }",
  "rootCause":  "{ ... root cause JSON ... }",
  "fix":        "### EXPLANATION\n...",
  "deployPlan": "{ ... deploy JSON ... }"
}
```

### Single agent (ad-hoc)

```http
POST /api/agents/invoke
Content-Type: application/json

{
  "agentName": "monitoring-agent",
  "input": "<paste logs here>"
}
```

### Agent catalogue

```http
GET /api/agents/catalogue
```

### MCP tool discovery

```http
GET /mcp/tools
```

### MCP tool invocation

```http
POST /mcp/tools/monitor
POST /mcp/tools/rootcause
POST /mcp/tools/fix
POST /mcp/tools/deploy
```

Body for all MCP endpoints:

```json
{ "input": "<text>" }
```

---

## Adding a New Agent (No Code Required)

1. Create `specs/security-agent.yaml`
2. Define `name`, `description`, `inputs`, `prompt`
3. Restart the API (or implement hot-reload)
4. The agent is automatically available at `/api/agents/invoke` and `/mcp/tools`

```yaml
name: SecurityAgent
description: Scan for OWASP Top-10 vulnerabilities in generated code
inputs:
  - fix
max_tokens: 1500
prompt: |
  You are a security engineer. Review the provided C# fix for vulnerabilities.
  ...
```

---

## Hackathon Demo Flow (2 minutes)

See [DEMO_SCRIPT.md](DEMO_SCRIPT.md) for the step-by-step presenter guide.

---

## Scoring Criteria Alignment

| Judge criterion | DevGuardian implementation |
|-----------------|---------------------------|
| Agentic AI | 4-stage autonomous pipeline with no human in the loop |
| Spec-driven / MCP | YAML specs + `/mcp/tools` endpoints |
| A2A interoperability | Any MCP client can call DevGuardian agents |
| Extensibility | New agent = new YAML file only |
| Azure OpenAI | Semantic Kernel + Azure OpenAI gpt-4o |
| Real-world value | Incident → root cause → fix → PR in < 30 s |

---

## License

MIT
