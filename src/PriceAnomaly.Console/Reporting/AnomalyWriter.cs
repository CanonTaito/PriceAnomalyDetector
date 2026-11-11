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
            _writer.WriteLine($"{abnormalValue.FileName} {abnormalValue.Date} {abnormalValue.Value} {abnormalValue.Median}");
        }
    }
}
