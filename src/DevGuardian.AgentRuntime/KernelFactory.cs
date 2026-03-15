using Microsoft.SemanticKernel;

namespace DevGuardian.AgentRuntime;

/// <summary>
/// Builds and configures a Semantic Kernel instance.
/// Pull configuration from appsettings / environment variables so
/// no secrets are hard-coded.
/// </summary>
public static class KernelFactory
{
    /// <summary>
    /// Creates a Kernel connected to Azure OpenAI.
    /// Environment variables (or appsettings) expected:
    ///   AZURE_OPENAI_ENDPOINT  – e.g. https://my-resource.openai.azure.com/
    ///   AZURE_OPENAI_KEY       – API key
    ///   AZURE_OPENAI_DEPLOYMENT – deployment / model name (default: gpt-4o)
    /// </summary>
    public static Kernel CreateFromEnvironment()
    {
        var endpoint   = Env("AZURE_OPENAI_ENDPOINT");
        var key        = Env("AZURE_OPENAI_KEY");
        var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4o";

        return Create(endpoint, key, deployment);
    }

    /// <summary>Creates a Kernel with explicit parameters.</summary>
    public static Kernel Create(string endpoint, string apiKey, string deployment = "gpt-4o")
    {
        var builder = Kernel.CreateBuilder();

        builder.AddAzureOpenAIChatCompletion(
            deploymentName: deployment,
            endpoint:       endpoint,
            apiKey:         apiKey);

        return builder.Build();
    }

    private static string Env(string name) =>
        Environment.GetEnvironmentVariable(name)
        ?? throw new InvalidOperationException(
            $"Required environment variable '{name}' is not set.");
}
