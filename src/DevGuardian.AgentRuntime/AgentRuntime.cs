using DevGuardian.AgentRuntime.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace DevGuardian.AgentRuntime;

/// <summary>
/// Executes agent specs dynamically against a configured Semantic Kernel.
/// Each call is stateless – the kernel is shared but execution context is not.
/// </summary>
public class AgentRuntime
{
    private readonly Kernel _kernel;
    private readonly ILogger<AgentRuntime> _logger;

    public AgentRuntime(Kernel kernel, ILogger<AgentRuntime> logger)
    {
        _kernel = kernel;
        _logger = logger;
    }

    /// <summary>
    /// Runs an agent spec with a plain string input.
    /// Variable substitution replaces {{input}} in the prompt.
    /// </summary>
    public async Task<string> ExecuteAsync(AgentSpec spec, string input,
        CancellationToken ct = default)
    {
        return await ExecuteAsync(spec, new Dictionary<string, string>
        {
            ["input"] = input
        }, ct);
    }

    /// <summary>
    /// Runs an agent spec with named variables that replace
    /// {{variable_name}} placeholders in the prompt template.
    /// </summary>
    public async Task<string> ExecuteAsync(AgentSpec spec,
        Dictionary<string, string> variables,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Executing agent: {Name}", spec.Name);

        // Build final prompt by substituting {{key}} placeholders
        var prompt = spec.Prompt;
        foreach (var (key, value) in variables)
            prompt = prompt.Replace($"{{{{{key}}}}}", value,
                StringComparison.OrdinalIgnoreCase);

        // Append all variable content after the template
        var inputSection = string.Join("\n",
            variables.Select(kv => $"### {kv.Key}\n{kv.Value}"));

        var fullPrompt = prompt + "\n\n" + inputSection;

        // Execution settings
        var settings = new AzureOpenAIPromptExecutionSettings
        {
            MaxTokens          = spec.MaxTokens,
            Temperature        = 0.2,
            TopP               = 0.95,
            ToolCallBehavior   = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var result = await _kernel.InvokePromptAsync(
            fullPrompt,
            new KernelArguments(settings),
            cancellationToken: ct);

        var output = result.ToString();
        _logger.LogInformation("Agent {Name} completed. Output length: {Len}",
            spec.Name, output.Length);

        return output;
    }
}
