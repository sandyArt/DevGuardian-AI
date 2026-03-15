using DevGuardian.AgentRuntime.Models;

namespace DevGuardian.AgentRuntime.Models;

/// <summary>
/// Strongly-typed representation of an agent YAML specification.
/// </summary>
public class AgentSpec
{
    /// <summary>Unique agent identifier (e.g. "MonitoringAgent").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Human-readable purpose of the agent.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Ordered list of input variable names the agent expects.</summary>
    public List<string> Inputs { get; set; } = new();

    /// <summary>
    /// System / user prompt template.
    /// Supports {{variable}} placeholders that are replaced at runtime.
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>Optional tools/plugins this agent is allowed to invoke.</summary>
    public List<AgentTool> Tools { get; set; } = new();

    /// <summary>
    /// Maximum tokens for this agent's response.
    /// Defaults to 2048 when not specified in the spec.
    /// </summary>
    public int MaxTokens { get; set; } = 2048;
}
