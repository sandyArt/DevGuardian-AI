using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace DevGuardian.Tools;

/// <summary>
/// Semantic Kernel plugin that interacts with GitHub.
/// In production, swap the stub implementations with Octokit calls.
/// </summary>
public class GitHubTool
{
    private readonly string _token;
    private readonly string _owner;
    private readonly string _repo;

    public GitHubTool(string token, string owner, string repo)
    {
        _token = token;
        _owner = owner;
        _repo  = repo;
    }

    [KernelFunction("create_pull_request")]
    [Description("Creates a pull request in the configured GitHub repository with the provided fix diff.")]
    public async Task<string> CreatePullRequestAsync(
        [Description("Branch name for the fix")] string branchName,
        [Description("Title of the pull request")] string title,
        [Description("Body / description of the pull request")] string body,
        [Description("The code diff or patch to include")] string diff)
    {
        // ----- Stub: replace with real Octokit calls -----
        await Task.Delay(50); // simulate I/O

        var prUrl = $"https://github.com/{_owner}/{_repo}/pull/999";
        return $"""
            Pull request created successfully.
            Title  : {title}
            Branch : {branchName}
            URL    : {prUrl}
            """;
    }

    [KernelFunction("list_open_issues")]
    [Description("Returns a list of open issues in the repository that match the provided label.")]
    public async Task<string> ListOpenIssuesAsync(
        [Description("GitHub label to filter issues (e.g. 'bug')")] string label = "bug")
    {
        await Task.Delay(50);
        return $"[Stub] Open issues with label '{label}' in {_owner}/{_repo}: #101, #102, #115";
    }
}
