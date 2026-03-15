using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace DevGuardian.Tools;

/// <summary>
/// Semantic Kernel plugin for log parsing and diagnostics utilities.
/// Agents can call these functions to enrich their analysis.
/// </summary>
public class DiagnosticsTool
{
    [KernelFunction("extract_errors")]
    [Description("Parses raw log text and returns a structured list of error entries (timestamp, level, message).")]
    public string ExtractErrors(
        [Description("Raw log text to parse")] string logs)
    {
        var pattern = new Regex(
            @"(?<ts>\d{4}-\d{2}-\d{2}[T ]\d{2}:\d{2}:\d{2})\s+(?<level>ERROR|FATAL|CRITICAL)[^\n]*",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        var matches = pattern.Matches(logs);
        if (matches.Count == 0)
            return "No ERROR/FATAL/CRITICAL entries found in the provided logs.";

        var lines = matches.Select(m =>
            $"[{m.Groups["ts"].Value}] {m.Groups["level"].Value.ToUpper()} – {m.Value.Trim()}");

        return string.Join("\n", lines);
    }

    [KernelFunction("calculate_error_rate")]
    [Description("Calculates the error rate percentage from raw logs (errors / total log lines * 100).")]
    public string CalculateErrorRate(
        [Description("Raw log text")] string logs)
    {
        var lines       = logs.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var errorCount  = lines.Count(l =>
            l.Contains("ERROR",    StringComparison.OrdinalIgnoreCase) ||
            l.Contains("FATAL",    StringComparison.OrdinalIgnoreCase) ||
            l.Contains("CRITICAL", StringComparison.OrdinalIgnoreCase));

        var rate = lines.Length == 0 ? 0d : errorCount * 100.0 / lines.Length;
        return $"Total lines: {lines.Length} | Errors: {errorCount} | Error rate: {rate:F2}%";
    }

    [KernelFunction("get_stack_trace")]
    [Description("Extracts the first stack trace found in the log text.")]
    public string GetStackTrace(
        [Description("Raw log text")] string logs)
    {
        var start = logs.IndexOf("   at ", StringComparison.Ordinal);
        if (start < 0)
            return "No stack trace found in the provided logs.";

        var end = logs.IndexOf("\n\n", start, StringComparison.Ordinal);
        return end < 0 ? logs[start..] : logs[start..end];
    }
}
