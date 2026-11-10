using PriceAnomaly.Console.Analysis;
using Xunit.Abstractions;

namespace PriceAnomaly.Tests.Analysis;

public class MedianCalculatorTests
{
    private readonly ITestOutputHelper _output;

    public MedianCalculatorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public static TheoryData<string, decimal[], decimal> MedianCases => new()
    {
        { "odd count", [3m, 1m, 2m], 2m },
        { "even count (avg of middle two)", [1m, 2m, 3m, 4m], 2.5m },
        { "single value", [7m], 7m },
        { "unsorted input", [10m, 1m, 5m], 5m },
        { "negatives", [-1m, -3m, -2m], -2m },
        { "duplicates and even", [5m, 1m, 5m, 2m], 3.5m },
    };

    [Theory]
    [MemberData(nameof(MedianCases))]
    public void Median_ReturnsExpected(string scenario, decimal[] values, decimal expected)
    {
        _output.WriteLine($"Scenario: {scenario}");

        var result = MedianCalculator.Median(values);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Median_EmptyInput_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => MedianCalculator.Median(Array.Empty<decimal>()));
    }
}
