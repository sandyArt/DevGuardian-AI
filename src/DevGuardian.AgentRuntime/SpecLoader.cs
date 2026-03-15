using DevGuardian.AgentRuntime.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DevGuardian.AgentRuntime;

/// <summary>
/// Loads and caches AgentSpec objects from YAML files on disk.
/// </summary>
public class SpecLoader
{
    private static readonly IDeserializer Deserializer =
        new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

    private readonly string _specsRoot;
    private readonly Dictionary<string, AgentSpec> _cache = new();

    /// <param name="specsRoot">
    /// Base folder that contains agent YAML files.
    /// Defaults to "specs" relative to the working directory.
    /// </param>
    public SpecLoader(string? specsRoot = null)
    {
        _specsRoot = specsRoot ?? Path.Combine(AppContext.BaseDirectory, "specs");
    }

    /// <summary>Loads a spec by file name (without extension).</summary>
    public AgentSpec Load(string specName)
    {
        if (_cache.TryGetValue(specName, out var cached))
            return cached;

        var candidates = new[]
        {
            Path.Combine(_specsRoot, $"{specName}.yaml"),
            Path.Combine(_specsRoot, $"{specName}.yml"),
            Path.Combine(_specsRoot, specName) // allow passing full filename
        };

        var path = candidates.FirstOrDefault(File.Exists)
            ?? throw new FileNotFoundException(
                $"Agent spec '{specName}' not found under '{_specsRoot}'.");

        var yaml = File.ReadAllText(path);
        var spec = Deserializer.Deserialize<AgentSpec>(yaml);
        _cache[specName] = spec;
        return spec;
    }

    /// <summary>Loads all *.yaml specs from the specs root folder.</summary>
    public IEnumerable<AgentSpec> LoadAll()
    {
        if (!Directory.Exists(_specsRoot))
            yield break;

        foreach (var file in Directory.EnumerateFiles(_specsRoot, "*.yaml"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            yield return Load(name);
        }
    }
}
