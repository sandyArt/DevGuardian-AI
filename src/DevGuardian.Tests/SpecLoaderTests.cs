using DevGuardian.AgentRuntime;
using DevGuardian.AgentRuntime.Models;
using Xunit;

namespace DevGuardian.Tests;

public class SpecLoaderTests
{
    private readonly string _fixturesPath = Path.Combine(
        AppContext.BaseDirectory, "Fixtures", "specs");

    [Fact]
    public void Load_ValidSpec_ReturnsPopulatedAgentSpec()
    {
        var loader = new SpecLoader(_fixturesPath);

        var spec = loader.Load("fixture-agent");

        Assert.Equal("FixtureAgent", spec.Name);
        Assert.Equal("A minimal agent used only in unit tests", spec.Description);
        Assert.Contains("input", spec.Inputs);
        Assert.Equal(256, spec.MaxTokens);
        Assert.NotEmpty(spec.Prompt);
    }

    [Fact]
    public void Load_MissingSpec_ThrowsFileNotFoundException()
    {
        var loader = new SpecLoader(_fixturesPath);

        Assert.Throws<FileNotFoundException>(() => loader.Load("nonexistent-agent"));
    }

    [Fact]
    public void Load_SameSpecTwice_ReturnsCachedInstance()
    {
        var loader = new SpecLoader(_fixturesPath);

        var first  = loader.Load("fixture-agent");
        var second = loader.Load("fixture-agent");

        Assert.Same(first, second);
    }

    [Fact]
    public void LoadAll_ReturnsAllSpecsInDirectory()
    {
        var loader = new SpecLoader(_fixturesPath);

        var specs = loader.LoadAll().ToList();

        Assert.NotEmpty(specs);
        Assert.All(specs, s => Assert.False(string.IsNullOrEmpty(s.Name)));
    }
}
