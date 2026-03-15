namespace DevGuardian.AgentRuntime.Models;

/// <summary>
/// Describes a single callable tool available to an agent,
/// sourced from the agent spec YAML.
/// </summary>
public class AgentTool
{
    /// <summary>Plugin name matching the registered Semantic Kernel plugin.</summary>
    public string Plugin { get; set; } = string.Empty;

    /// <summary>Function name within the plugin.</summary>
    public string Function { get; set; } = string.Empty;

    /// <summary>Human-readable description used in the agent prompt.</summary>
    public string Description { get; set; } = string.Empty;
}
