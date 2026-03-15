using DevGuardian.AgentRuntime.Models;
using Microsoft.Extensions.Logging;

namespace DevGuardian.AgentRuntime;

/// <summary>
/// Orchestrates the full spec-driven pipeline:
///   logs → MonitoringAgent → RootCauseAgent → FixAgent → (optional) DeployAgent
/// Each step's output becomes the next step's input.
/// </summary>
public class WorkflowEngine
{
    private readonly AgentRuntime _runtime;
    private readonly SpecLoader _loader;
    private readonly ILogger<WorkflowEngine> _logger;

    public WorkflowEngine(
        AgentRuntime runtime,
        SpecLoader loader,
        ILogger<WorkflowEngine> logger)
    {
        _runtime = runtime;
        _loader  = loader;
        _logger  = logger;
    }

    /// <summary>
    /// Runs the full four-stage DevGuardian pipeline.
    /// Returns a <see cref="WorkflowResult"/> with the intermediate outputs
    /// from every stage so the API can surface them independently.
    /// </summary>
    public async Task<WorkflowResult> RunAsync(string logs,
        CancellationToken ct = default)
    {
        _logger.LogInformation("=== DevGuardian workflow started ===");

        // Stage 1: Detect the incident
        var monitorSpec = _loader.Load("monitoring-agent");
        var incident    = await _runtime.ExecuteAsync(monitorSpec, logs, ct);
        _logger.LogInformation("Stage 1 complete – incident detected.");

        // Stage 2: Root-cause analysis
        var rootSpec = _loader.Load("rootcause-agent");
        var rootCause = await _runtime.ExecuteAsync(rootSpec, incident, ct);
        _logger.LogInformation("Stage 2 complete – root cause identified.");

        // Stage 3: Fix generation
        var fixSpec = _loader.Load("fix-agent");
        var fix = await _runtime.ExecuteAsync(fixSpec,
            new Dictionary<string, string>
            {
                ["root_cause"] = rootCause,
                ["incident"]   = incident
            }, ct);
        _logger.LogInformation("Stage 3 complete – fix generated.");

        // Stage 4: Validation / deploy readiness
        var deploySpec  = _loader.Load("deploy-agent");
        var deployPlan  = await _runtime.ExecuteAsync(deploySpec,
            new Dictionary<string, string>
            {
                ["fix"]       = fix,
                ["root_cause"] = rootCause
            }, ct);
        _logger.LogInformation("Stage 4 complete – deploy plan produced.");

        return new WorkflowResult
        {
            Incident   = incident,
            RootCause  = rootCause,
            Fix        = fix,
            DeployPlan = deployPlan
        };
    }

    /// <summary>
    /// Runs the optional 5th stage: SecurityAgent reviews the generated fix.
    /// Can be called independently after <see cref="RunAsync"/>.
    /// </summary>
    public async Task<string> RunSecurityReviewAsync(string fix,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Running optional security review stage.");

        try
        {
            var secSpec  = _loader.Load("security-agent");
            var review   = await _runtime.ExecuteAsync(secSpec,
                new Dictionary<string, string> { ["fix"] = fix }, ct);

            _logger.LogInformation("Security review complete.");
            return review;
        }
        catch (FileNotFoundException)
        {
            _logger.LogWarning("security-agent.yaml not found – skipping security review.");
            return """{"risk_level":"SKIP","reviewer_notes":"security-agent.yaml not present."}""";
        }
    }
}

/// <summary>Immutable result from a completed DevGuardian workflow run.</summary>
public record WorkflowResult
{
    public string Incident       { get; init; } = string.Empty;
    public string RootCause      { get; init; } = string.Empty;
    public string Fix            { get; init; } = string.Empty;
    public string DeployPlan     { get; init; } = string.Empty;
    public string? SecurityReview { get; init; }
}
