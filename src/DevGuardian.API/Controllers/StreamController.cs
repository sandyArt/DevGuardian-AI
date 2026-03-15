using DevGuardian.AgentRuntime;
using DevGuardian.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace DevGuardian.API.Controllers;

/// <summary>
/// Streaming endpoint: sends each agent's result as a Server-Sent Event
/// so the dashboard can animate stage-by-stage in real time.
/// </summary>
[ApiController]
[Route("api/agents")]
public class StreamController : ControllerBase
{
    private readonly AgentRuntime.AgentRuntime _runtime;
    private readonly SpecLoader _loader;

    public StreamController(AgentRuntime.AgentRuntime runtime, SpecLoader loader)
    {
        _runtime = runtime;
        _loader  = loader;
    }

    // ------------------------------------------------------------------
    // GET /api/agents/stream?logs=<encoded>
    // POST /api/agents/stream  body: { "logs": "..." }
    // ------------------------------------------------------------------
    /// <summary>
    /// Runs the 4-stage DevGuardian pipeline and streams each stage
    /// result as a Server-Sent Event.
    ///
    /// SSE event types:
    ///   stage_start  – agent name started
    ///   stage_done   – agent completed with result payload
    ///   pipeline_done – all stages finished
    ///   error         – fatal error, stream closes
    /// </summary>
    [HttpPost("stream")]
    public async Task Stream(
        [FromBody] RunWorkflowRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Logs))
        {
            Response.StatusCode = 400;
            await Response.WriteAsync("Logs must not be empty.", ct);
            return;
        }

        Response.Headers["Content-Type"]  = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["X-Accel-Buffering"] = "no"; // Nginx pass-through

        var stages = new[]
        {
            ("monitoring-agent", "MonitoringAgent",  request.Logs),
            ("rootcause-agent",  "RootCauseAgent",   null),
            ("fix-agent",        "FixAgent",          null),
            ("deploy-agent",     "DeployAgent",       null)
        };

        string previousOutput = request.Logs;

        try
        {
            for (int i = 0; i < stages.Length; i++)
            {
                ct.ThrowIfCancellationRequested();

                var (specName, label, _) = stages[i];

                // Notify dashboard that this stage is starting
                await SendEvent(Response, "stage_start", new
                {
                    stage = i,
                    name  = label
                }, ct);

                var spec = _loader.Load(specName);

                // Pass previous stage output as the input to the next
                Dictionary<string, string> vars = i switch
                {
                    0 => new() { ["input"] = request.Logs },
                    1 => new() { ["input"] = previousOutput },
                    2 => new() { ["root_cause"] = previousOutput, ["incident"] = request.Logs },
                    3 => new() { ["fix"] = previousOutput, ["root_cause"] = previousOutput },
                    _ => new() { ["input"] = previousOutput }
                };

                var output = await _runtime.ExecuteAsync(spec, vars, ct);
                previousOutput = output;

                // Send the completed stage result
                await SendEvent(Response, "stage_done", new
                {
                    stage  = i,
                    name   = label,
                    result = output
                }, ct);

                await Response.Body.FlushAsync(ct);
            }

            await SendEvent(Response, "pipeline_done", new
            {
                message = "All stages completed successfully."
            }, ct);
        }
        catch (OperationCanceledException)
        {
            // Client disconnected – normal
        }
        catch (Exception ex)
        {
            await SendEvent(Response, "error", new
            {
                message = ex.Message
            }, ct);
        }
        finally
        {
            await Response.Body.FlushAsync(CancellationToken.None);
        }
    }

    private static async Task SendEvent(HttpResponse response, string eventType,
        object data, CancellationToken ct)
    {
        var json     = JsonSerializer.Serialize(data,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var payload  = $"event: {eventType}\ndata: {json}\n\n";
        var bytes    = Encoding.UTF8.GetBytes(payload);
        await response.Body.WriteAsync(bytes, ct);
    }
}
