namespace PriceAnomaly.Console.Analysis;

public static class MedianCalculator
{
    public static decimal Median(IReadOnlyList<decimal> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count == 0)
            throw new ArgumentException("Values cannot be empty.", nameof(values));

        var sorted = values.OrderBy(v => v).ToArray();
        var mid = sorted.Length / 2;

        if (sorted.Length % 2 == 1)
            return sorted[mid];

        return (sorted[mid - 1] + sorted[mid]) / 2m;
    }
}
