# DevGuardian AI – 2-Minute Demo Script

> **Goal:** Show judges a live end-to-end flow from pasted logs to a GitHub PR plan
> in under 2 minutes. Rehearse once before the presentation.

---

## Setup (before judges arrive)

- [ ] API running: `dotnet run --project src/DevGuardian.API`
- [ ] Browser open at `http://localhost:5000` (Swagger UI)
- [ ] Sample log snippet ready (copy block below)
- [ ] Terminal visible on second monitor (optional)

### Sample log snippet

```
2024-03-12T10:04:58 INFO  PaymentService started
2024-03-12T10:05:01 ERROR PaymentService.cs line 210 NullReferenceException: Object reference not set
   at PaymentService.ProcessPayment(Order order) in PaymentService.cs:line 210
   at OrderController.Checkout(CheckoutRequest req) in OrderController.cs:line 87
2024-03-12T10:05:02 ERROR PaymentService.cs line 210 NullReferenceException: Object reference not set
2024-03-12T10:05:03 ERROR PaymentService.cs line 210 NullReferenceException: Object reference not set
2024-03-12T10:05:04 WARN  Retry attempt 1/3 for order #4821
2024-03-12T10:05:07 ERROR PaymentService.cs line 210 NullReferenceException: Object reference not set
2024-03-12T10:05:07 FATAL Circuit breaker OPEN – PaymentService unavailable
```

---

## Talking Track

### [0:00 – 0:15] Hook

> "Imagine your payment service just went down at 2 AM.
> DevGuardian AI takes raw logs and autonomously finds the bug,
> generates the fix, and drafts the pull request – in under 30 seconds."

### [0:15 – 0:30] Architecture slide

Point to the diagram:

> "Four specialist AI agents, each defined by a YAML spec.
> The Monitoring Agent detects the incident.
> Root Cause identifies the exact file and line.
> Fix Agent writes the C# patch.
> Deploy Agent produces the rollout plan.
> No agent hardcodes behaviour – swap the YAML, change the agent."

### [0:30 – 1:15] Live demo – Swagger

1. Open **POST /api/agents/run** in Swagger
2. Click **Try it out**
3. Paste the sample log snippet
4. Click **Execute**
5. Point to the response fields live:
   - `incident` → *"MonitoringAgent detected a NullReferenceException spike – CRITICAL severity"*
   - `rootCause` → *"PaymentService.cs line 210 – ProcessPayment method"*
   - `fix` → *"Here's the BEFORE/AFTER C# patch with a null guard and an xUnit test"*
   - `deployPlan` → *"Pre-check: run unit tests. Deploy: blue/green. Rollback within 2 minutes."*

### [1:15 – 1:35] MCP slide

> "Every agent is also an MCP tool.
> Any MCP-compatible host – GitHub Copilot, another AI agent, your CI pipeline –
> can call GET /mcp/tools to discover them and POST to invoke them.
> This is true agent-to-agent interoperability."

Show in browser: **GET /mcp/tools** → list of tools with input schemas.

### [1:35 – 1:50] Visual Agent Builder

> "Want to add a Security Agent?
> Just drop a new YAML file in the specs folder.
> No code. No recompile in production.
> The runtime picks it up automatically."

Show `specs/fix-agent.yaml` briefly – highlight `name`, `prompt`, `inputs`.

### [1:50 – 2:00] Close

> "DevGuardian AI turns a 2-hour incident response cycle
> into a 30-second automated loop.
> Spec-driven. MCP-compatible. Extensible without code changes.
> Thank you."

---

## Backup slides (if live demo fails)

- Screenshot of Swagger response (pre-captured)
- Architecture diagram PNG
- `specs/monitoring-agent.yaml` open in VS Code

---

## Judge Q&A Cheat Sheet

| Question | Answer |
|----------|--------|
| "How do you prevent hallucinated fixes?" | Structured JSON output + confidence score from RootCauseAgent; fix always requires human PR review |
| "What if we want to add a new agent?" | Drop a YAML file in `specs/` – no code change needed |
| "How is this different from just calling GPT directly?" | Spec-driven architecture separates *behaviour* (YAML) from *execution* (runtime). Any agent can be versioned, A/B tested, or replaced without touching C# |
| "Does this work with non-Azure OpenAI?" | Yes – swap `KernelFactory.Create()` for `AddOpenAIChatCompletion()` – Semantic Kernel is provider-agnostic |
| "Is it MCP-compliant?" | The `/mcp/tools` endpoints follow the MCP tool-manifest convention; full schema validation can be added in 30 min |
