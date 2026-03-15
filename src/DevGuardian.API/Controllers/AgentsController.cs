using DevGuardian.AgentRuntime;
using DevGuardian.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace DevGuardian.API.Controllers;

/// <summary>
/// Main API surface for DevGuardian agent operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AgentsController : ControllerBase
{
    private readonly WorkflowEngine _engine;
    private readonly AgentRuntime.AgentRuntime _runtime;
    private readonly SpecLoader _loader;

    public AgentsController(
        WorkflowEngine engine,
        AgentRuntime.AgentRuntime runtime,
        SpecLoader loader)
    {
        _engine  = engine;
        _runtime = runtime;
        _loader  = loader;
    }

    // ------------------------------------------------------------------
    // POST /api/agents/run
    // ------------------------------------------------------------------
    /// <summary>
    /// Runs the full four-stage DevGuardian pipeline against the supplied logs.
    /// Returns incident, root-cause, fix, and deploy plan as separate fields.
    /// </summary>
    [HttpPost("run")]
    [ProducesResponseType(typeof(WorkflowResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Run(
        [FromBody] RunWorkflowRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Logs))
            return BadRequest("Logs must not be empty.");

        var result = await _engine.RunAsync(request.Logs, ct);

        // Optionally run security review if spec is present
        var secReview = await _engine.RunSecurityReviewAsync(result.Fix, ct);

        return Ok(new WorkflowResponse
        {
            Incident       = result.Incident,
            RootCause      = result.RootCause,
            Fix            = result.Fix,
            DeployPlan     = result.DeployPlan,
            SecurityReview = secReview
        });
    }

    // ------------------------------------------------------------------
    // POST /api/agents/invoke
    // ------------------------------------------------------------------
    /// <summary>
    /// Invokes a single agent by spec name with arbitrary input.
    /// Useful for ad-hoc testing of individual agents.
    /// </summary>
    [HttpPost("invoke")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Invoke(
        [FromBody] RunAgentRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.AgentName))
            return BadRequest("AgentName must not be empty.");

        AgentRuntime.Models.AgentSpec spec;
        try
        {
            spec = _loader.Load(request.AgentName);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(ex.Message);
        }

        string output;
        if (request.Variables is { Count: > 0 })
            output = await _runtime.ExecuteAsync(spec, request.Variables, ct);
        else
            output = await _runtime.ExecuteAsync(spec, request.Input, ct);

        return Ok(output);
    }

    // ------------------------------------------------------------------
    // GET /api/agents/catalogue
    // ------------------------------------------------------------------
    /// <summary>
    /// Returns metadata for all registered agent specs.
    /// </summary>
    [HttpGet("catalogue")]
    [ProducesResponseType(typeof(IEnumerable<AgentInfo>), StatusCodes.Status200OK)]
    public IActionResult Catalogue()
    {
        var agents = _loader.LoadAll().Select(s => new AgentInfo
        {
            Name        = s.Name,
            Description = s.Description,
            Inputs      = s.Inputs
        });

        return Ok(agents);
    }
}
