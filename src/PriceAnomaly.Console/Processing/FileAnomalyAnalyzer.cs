using PriceAnomaly.Console.Analysis;
using PriceAnomaly.Console.FileTypes;
using PriceAnomaly.Console.IO;
using PriceAnomaly.Console.Models;

namespace PriceAnomaly.Console.Processing;

public sealed class FileAnomalyAnalyzer
{
    private readonly CsvRowReader _reader;
    private readonly FileTypeResolver _resolver;

    public FileAnomalyAnalyzer(CsvRowReader reader, FileTypeResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(resolver);
        _reader = reader;
        _resolver = resolver;
    }

    public IReadOnlyList<AbnormalValue> Analyze(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var definition = _resolver.Resolve(path);
        var rows = _reader.Read(path, definition.ValueColumn).ToList();
        if (rows.Count == 0)
            return [];

        var median = MedianCalculator.Median(rows.Select(row => row.Value).ToList());
        return ThresholdAnomalyDetector.Detect(Path.GetFileName(path), rows, median);
    }
}
