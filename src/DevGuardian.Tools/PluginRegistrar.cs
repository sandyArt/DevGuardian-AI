using Microsoft.SemanticKernel;

namespace DevGuardian.Tools;

/// <summary>
/// Convenience class that registers all DevGuardian tool plugins
/// into a Semantic Kernel instance.
/// </summary>
public static class PluginRegistrar
{
    /// <summary>
    /// Registers <see cref="DiagnosticsTool"/> and <see cref="GitHubTool"/>
    /// on the provided kernel.
    /// </summary>
    /// <param name="kernel">Target Semantic Kernel instance.</param>
    /// <param name="githubToken">GitHub personal access token.</param>
    /// <param name="githubOwner">Repository owner / organisation.</param>
    /// <param name="githubRepo">Repository name.</param>
    public static Kernel RegisterAll(
        Kernel kernel,
        string githubToken  = "",
        string githubOwner  = "your-org",
        string githubRepo   = "your-repo")
    {
        kernel.ImportPluginFromObject(new DiagnosticsTool(), "Diagnostics");
        kernel.ImportPluginFromObject(
            new GitHubTool(githubToken, githubOwner, githubRepo),
            "GitHub");

        return kernel;
    }
}
