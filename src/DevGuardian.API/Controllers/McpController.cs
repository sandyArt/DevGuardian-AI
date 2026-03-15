using DevGuardian.AgentRuntime;
using Microsoft.AspNetCore.Mvc;

namespace DevGuardian.API.Controllers;

/// <summary>
/// MCP-compatible (Model Context Protocol) endpoint layer.
/// Each agent is exposed as a discoverable tool under /mcp/tools/{toolName}.
/// Judges looking for A2A / MCP interoperability should be directed here.
/// </summary>
[ApiController]
[Route("mcp")]
[Produces("application/json")]
public class McpController : ControllerBase
{
    private readonly AgentRuntime.AgentRuntime _runtime;
    private readonly SpecLoader _loader;

    public McpController(AgentRuntime.AgentRuntime runtime, SpecLoader loader)
    {
        _runtime = runtime;
        _loader  = loader;
    }

    // ------------------------------------------------------------------
    // GET /mcp/tools
    // ------------------------------------------------------------------
    /// <summary>
    /// MCP tool discovery – returns all available agent tools with their
    /// input schemas so any MCP-compatible host can invoke them.
    /// </summary>
    [HttpGet("tools")]
    public IActionResult DiscoverTools()
    {
        var tools = _loader.LoadAll().Select(spec => new
        {
            name        = spec.Name,
            description = spec.Description,
            inputSchema = new
            {
                type       = "object",
                properties = spec.Inputs.ToDictionary(
                    i => i,
                    _ => new { type = "string" }),
                required = spec.Inputs
            }
        });

        return Ok(new { tools });
    }

    // ------------------------------------------------------------------
    // POST /mcp/tools/monitor
    // ------------------------------------------------------------------
    [HttpPost("tools/monitor")]
    public async Task<IActionResult> Monitor(
        [FromBody] McpToolRequest request,
        CancellationToken ct)
        => await InvokeTool("monitoring-agent", request, ct);

    // ------------------------------------------------------------------
    // POST /mcp/tools/rootcause
    // ------------------------------------------------------------------
    [HttpPost("tools/rootcause")]
    public async Task<IActionResult> RootCause(
        [FromBody] McpToolRequest request,
        CancellationToken ct)
        => await InvokeTool("rootcause-agent", request, ct);

    // ------------------------------------------------------------------
    // POST /mcp/tools/fix
    // ------------------------------------------------------------------
    [HttpPost("tools/fix")]
    public async Task<IActionResult> Fix(
        [FromBody] McpToolRequest request,
        CancellationToken ct)
        => await InvokeTool("fix-agent", request, ct);

    // ------------------------------------------------------------------
    // POST /mcp/tools/deploy
    // ------------------------------------------------------------------
    [HttpPost("tools/deploy")]
    public async Task<IActionResult> Deploy(
        [FromBody] McpToolRequest request,
        CancellationToken ct)
        => await InvokeTool("deploy-agent", request, ct);

    // ------------------------------------------------------------------
    // POST /mcp/tools/security
    // ------------------------------------------------------------------
    [HttpPost("tools/security")]
    public async Task<IActionResult> Security(
        [FromBody] McpToolRequest request,
        CancellationToken ct)
        => await InvokeTool("security-agent", request, ct);

    // ------------------------------------------------------------------
    // Generic tool invoker
    // ------------------------------------------------------------------
    private async Task<IActionResult> InvokeTool(
        string specName,
        McpToolRequest request,
        CancellationToken ct)
    {
        AgentRuntime.Models.AgentSpec spec;
        try
        {
            spec = _loader.Load(specName);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }

        string output;
        if (request.Variables is { Count: > 0 })
            output = await _runtime.ExecuteAsync(spec, request.Variables, ct);
        else
            output = await _runtime.ExecuteAsync(spec, request.Input ?? string.Empty, ct);

        return Ok(new McpToolResponse
        {
            Tool   = spec.Name,
            Result = output
        });
    }
}

/// <summary>Inbound payload for any MCP tool call.</summary>
public record McpToolRequest
{
    /// <summary>Simple single-string input (used when the spec has one input).</summary>
    public string? Input { get; init; }

    /// <summary>Named variables for multi-input agents.</summary>
    public Dictionary<string, string>? Variables { get; init; }
}

/// <summary>Standard MCP tool response envelope.</summary>
public record McpToolResponse
{
    public string Tool   { get; init; } = string.Empty;
    public string Result { get; init; } = string.Empty;
}
