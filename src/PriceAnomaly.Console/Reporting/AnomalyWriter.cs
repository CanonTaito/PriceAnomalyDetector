using System.Globalization;
using PriceAnomaly.Console.Models;

namespace PriceAnomaly.Console.Reporting;

public sealed class AnomalyWriter
{
    private readonly TextWriter _writer;

    public AnomalyWriter(TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        _writer = writer;
    }

    public void Write(IEnumerable<AbnormalValue> abnormalities)
    {
        ArgumentNullException.ThrowIfNull(abnormalities);

        foreach (var abnormalValue in abnormalities)
        {
            // Invariant culture so output is identical regardless of the machine's regional settings.
            _writer.WriteLine(string.Create(
                CultureInfo.InvariantCulture,
                $"{abnormalValue.FileName} {abnormalValue.Date} {abnormalValue.Value} {abnormalValue.Median}"));
        }
    }
}
