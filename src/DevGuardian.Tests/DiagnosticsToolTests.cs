using DevGuardian.Tools;
using Xunit;

namespace DevGuardian.Tests;

public class DiagnosticsToolTests
{
    private readonly DiagnosticsTool _tool = new();

    private const string SampleLogs = """
        2024-03-12T10:04:58 INFO  Service started
        2024-03-12T10:05:01 ERROR PaymentService.cs line 210 NullReferenceException
           at PaymentService.Process() in PaymentService.cs:line 210
        2024-03-12T10:05:02 FATAL Circuit breaker OPEN
        2024-03-12T10:05:03 INFO  Health check OK
        2024-03-12T10:05:04 ERROR Another error occurred
        """;

    [Fact]
    public void ExtractErrors_WithErrorLines_ReturnsOnlyErrors()
    {
        var result = _tool.ExtractErrors(SampleLogs);

        Assert.Contains("ERROR", result);
        Assert.Contains("FATAL", result);
        Assert.DoesNotContain("INFO", result);
    }

    [Fact]
    public void ExtractErrors_WithNoErrors_ReturnsNotFoundMessage()
    {
        var result = _tool.ExtractErrors("INFO All is well\nDEBUG nothing to see here");

        Assert.Contains("No ERROR", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CalculateErrorRate_WithMixedLogs_ReturnsFormattedRate()
    {
        var result = _tool.CalculateErrorRate(SampleLogs);

        Assert.Contains("Error rate:", result);
        Assert.Contains("Total lines:", result);
        Assert.Contains("Errors:", result);
    }

    [Fact]
    public void CalculateErrorRate_EmptyInput_ReturnsZeroRate()
    {
        var result = _tool.CalculateErrorRate(string.Empty);

        Assert.Contains("0.00%", result);
    }

    [Fact]
    public void GetStackTrace_WithTrace_ExtractsFirstTrace()
    {
        var result = _tool.GetStackTrace(SampleLogs);

        Assert.Contains("at PaymentService", result);
    }

    [Fact]
    public void GetStackTrace_WithNoTrace_ReturnsNotFoundMessage()
    {
        var result = _tool.GetStackTrace("ERROR Something went wrong");

        Assert.Contains("No stack trace", result, StringComparison.OrdinalIgnoreCase);
    }
}
