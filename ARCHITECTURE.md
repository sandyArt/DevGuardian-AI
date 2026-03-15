# DevGuardian AI – Architecture Diagrams

## Use in presentations

The diagrams below are in Mermaid format. Render them with:
- [mermaid.live](https://mermaid.live) → paste → export PNG/SVG
- GitHub renders Mermaid in Markdown automatically
- VS Code: install "Markdown Preview Mermaid Support" extension

---

## Diagram 1 – Full System Architecture

```mermaid
flowchart TD
    subgraph Client["Client Layer"]
        UI["🖥️ Web Dashboard\n(index.html + SSE)"]
        MCP_HOST["🤖 MCP Host\n(Copilot / other AI)"]
        SWAGGER["📄 Swagger UI"]
    end

    subgraph API["ASP.NET Core API  (.NET 8)"]
        WF["POST /api/agents/run"]
        STREAM["POST /api/agents/stream\nServer-Sent Events"]
        INVOKE["POST /api/agents/invoke"]
        CAT["GET /api/agents/catalogue"]
        MCP_DISC["GET /mcp/tools"]
        MCP_CALL["POST /mcp/tools/{name}"]
    end

    subgraph Runtime["DevGuardian.AgentRuntime"]
        ENGINE["WorkflowEngine\n5-stage orchestrator"]
        LOADER["SpecLoader\nYAML → AgentSpec (cached)"]
        AR["AgentRuntime\nSemanticKernel executor"]
    end

    subgraph Specs["Agent Specifications (YAML)"]
        S1["📋 monitoring-agent.yaml"]
        S2["🔍 rootcause-agent.yaml"]
        S3["🔧 fix-agent.yaml"]
        S4["🚀 deploy-agent.yaml"]
        S5["🛡️ security-agent.yaml"]
    end

    subgraph Plugins["Semantic Kernel Plugins"]
        P1["DiagnosticsTool\n(log parsing)"]
        P2["GitHubTool\n(PR creation)"]
    end

    subgraph Azure["Azure / AI Backend"]
        AOI["Azure OpenAI\ngpt-4o deployment"]
    end

    UI -->|"POST { logs }"| STREAM
    UI -->|"GET"| MCP_DISC
    MCP_HOST -->|"Tool discovery"| MCP_DISC
    MCP_HOST -->|"Tool invocation"| MCP_CALL
    SWAGGER --> WF & INVOKE & CAT

    WF --> ENGINE
    STREAM --> ENGINE
    MCP_CALL --> AR
    INVOKE --> AR

    ENGINE --> LOADER
    ENGINE --> AR
    LOADER --> S1 & S2 & S3 & S4 & S5

    AR -->|"InvokePromptAsync\nAutoInvokeKernelFunctions"| AOI
    AR --> P1 & P2

    style Specs fill:#1a2040,stroke:#3b82f6
    style Azure fill:#1a2040,stroke:#a855f7
    style Plugins fill:#1a2040,stroke:#22c55e
```

---

## Diagram 2 – Agent Data Flow

```mermaid
sequenceDiagram
    participant User
    participant API
    participant Monitor as MonitoringAgent
    participant RCA as RootCauseAgent
    participant Fix as FixAgent
    participant Deploy as DeployAgent
    participant Security as SecurityAgent
    participant AOAI as Azure OpenAI

    User->>API: POST /api/agents/stream { logs }
    API-->>User: SSE: stage_start (0)

    API->>Monitor: ExecuteAsync(monitoring-agent.yaml, logs)
    Monitor->>AOAI: InvokePromptAsync
    AOAI-->>Monitor: { service, severity, error_rate }
    Monitor-->>API: incident JSON
    API-->>User: SSE: stage_done (0, incident)

    API->>RCA: ExecuteAsync(rootcause-agent.yaml, incident)
    RCA->>AOAI: InvokePromptAsync
    AOAI-->>RCA: { file, line, method, cause }
    RCA-->>API: rootCause JSON
    API-->>User: SSE: stage_done (1, rootCause)

    API->>Fix: ExecuteAsync(fix-agent.yaml, rootCause+incident)
    Fix->>AOAI: InvokePromptAsync
    AOAI-->>Fix: C# BEFORE/AFTER patch
    Fix-->>API: fix markdown
    API-->>User: SSE: stage_done (2, fix)

    API->>Deploy: ExecuteAsync(deploy-agent.yaml, fix+rootCause)
    Deploy->>AOAI: InvokePromptAsync
    AOAI-->>Deploy: { pre_deployment, steps, rollback }
    Deploy-->>API: deployPlan JSON
    API-->>User: SSE: stage_done (3, deployPlan)

    API->>Security: ExecuteAsync(security-agent.yaml, fix)
    Security->>AOAI: InvokePromptAsync
    AOAI-->>Security: { risk_level, vulnerabilities, approved }
    Security-->>API: securityReview JSON

    API-->>User: SSE: pipeline_done
```

---

## Diagram 3 – MCP Integration

```mermaid
flowchart LR
    subgraph External["External MCP Hosts"]
        GHC["GitHub Copilot"]
        OTHER["Other AI Agents\n/ Orchestrators"]
    end

    subgraph DevGuardian["DevGuardian AI MCP Layer"]
        DISC["GET /mcp/tools\nTool manifest + inputSchema"]
        MON["POST /mcp/tools/monitor"]
        RCA["POST /mcp/tools/rootcause"]
        FIX["POST /mcp/tools/fix"]
        DEP["POST /mcp/tools/deploy"]
        SEC["POST /mcp/tools/security"]
    end

    GHC -->|"Discover tools"| DISC
    OTHER -->|"Discover tools"| DISC

    GHC -->|"Invoke tool"| MON & RCA & FIX & DEP & SEC
    OTHER -->|"Invoke tool"| MON & RCA & FIX & DEP & SEC

    DISC -->|"Returns schema"| GHC & OTHER
```
