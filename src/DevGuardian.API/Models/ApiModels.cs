namespace DevGuardian.API.Models;

/// <summary>Request body for the workflow run endpoint.</summary>
public record RunWorkflowRequest
{
    /// <summary>Raw log text to analyse (paste from stdout/file/etc.).</summary>
    public string Logs { get; init; } = string.Empty;
}

/// <summary>Request body for running a single named agent ad-hoc.</summary>
public record RunAgentRequest
{
    /// <summary>Name of the agent spec to run (without .yaml extension).</summary>
    public string AgentName { get; init; } = string.Empty;

    /// <summary>Input text to pass to the agent.</summary>
    public string Input { get; init; } = string.Empty;

    /// <summary>Optional named variables for multi-input agents.</summary>
    public Dictionary<string, string>? Variables { get; init; }
}

/// <summary>Response returned by the workflow run endpoint.</summary>
public record WorkflowResponse
{
    public string Incident        { get; init; } = string.Empty;
    public string RootCause       { get; init; } = string.Empty;
    public string Fix             { get; init; } = string.Empty;
    public string DeployPlan      { get; init; } = string.Empty;
    public string? SecurityReview { get; init; }
}

/// <summary>Details of a registered agent spec – used by the catalogue endpoint.</summary>
public record AgentInfo
{
    public string Name        { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public List<string> Inputs { get; init; } = new();
}
