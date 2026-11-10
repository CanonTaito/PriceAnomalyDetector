using PriceAnomaly.Console.Analysis;
using PriceAnomaly.Console.Models;
using Xunit.Abstractions;

namespace PriceAnomaly.Tests.Analysis;

public class ThresholdAnomalyDetectorTests
{
    private readonly ITestOutputHelper _output;

    public ThresholdAnomalyDetectorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public static TheoryData<string, decimal, decimal, decimal, bool> FlagCases => new()
    {
        { "more than 20% above", 100m, 121m, 0.20m, true },
        { "more than 20% below", 100m, 79m, 0.20m, true },
        { "exactly 20% above (not abnormal)", 100m, 120m, 0.20m, false },
        { "exactly 20% below (not abnormal)", 100m, 80m, 0.20m, false },
        { "within threshold", 100m, 110m, 0.20m, false },
        { "median zero, non-zero value (abnormal)", 0m, 5m, 0.20m, true },
        { "median zero, zero value (normal)", 0m, 0m, 0.20m, false },
        { "custom threshold respected", 100m, 111m, 0.10m, true },
    };

    [Theory]
    [MemberData(nameof(FlagCases))]
    public void Detect_FlagsValue_AsExpected(string scenario, decimal median, decimal value, decimal threshold, bool expected)
    {
        _output.WriteLine($"Scenario: {scenario}");

        var result = ThresholdAnomalyDetector.IsAbnormal(value, median, threshold);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Detect_ReturnsAbnormalValues_WithFileDateValueMedian()
    {
        var rows = new List<DataRow>
        {
            new("1/06/2021", 100m),
            new("2/06/2021", 130m),
            new("3/06/2021", 70m),
            new("4/06/2021", 110m),
        };

        var result = ThresholdAnomalyDetector.Detect("mod_01.csv", rows, median: 100m);

        var expected = new[]
        {
            new AbnormalValue("mod_01.csv", "2/06/2021", 130m, 100m),
            new AbnormalValue("mod_01.csv", "3/06/2021", 70m, 100m),
        };

        Assert.Equal(expected, result);
    }
}
