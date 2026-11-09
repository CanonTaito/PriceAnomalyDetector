using PriceAnomaly.Console.Models;

namespace PriceAnomaly.Console.Analysis;

public static class ThresholdAnomalyDetector
{
    public const decimal DefaultThreshold = 0.20m;

    public static bool IsAbnormal(decimal value, decimal median, decimal threshold = DefaultThreshold)
    {
        if (threshold < 0)
            throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold cannot be negative.");

        if (median == 0m)
            return value != 0m;

        var deviation = Math.Abs(value - median) / Math.Abs(median);
        return deviation > threshold;
    }

    public static IReadOnlyList<AbnormalValue> Detect(
        string fileName,
        IReadOnlyList<DataRow> rows,
        decimal median,
        decimal threshold = DefaultThreshold)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(rows);

        var abnormalities = new List<AbnormalValue>();
        foreach (var row in rows)
        {
            if (IsAbnormal(row.Value, median, threshold))
                abnormalities.Add(new AbnormalValue(fileName, row.Date, row.Value, median));
        }

        return abnormalities;
    }
}
