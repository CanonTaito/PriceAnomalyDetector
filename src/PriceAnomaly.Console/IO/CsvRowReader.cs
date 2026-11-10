using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using PriceAnomaly.Console.Models;

namespace PriceAnomaly.Console.IO;

public sealed class CsvRowReader
{
    private const string DateColumn = "Date";

    public IEnumerable<DataRow> Read(string path, string valueColumn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(valueColumn);

        return ReadRows(path, valueColumn);
    }

    private static IEnumerable<DataRow> ReadRows(string path, string valueColumn)
    {
        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            TrimOptions = TrimOptions.Trim,
            PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant(),
        };

        using var streamReader = new StreamReader(path);
        using var csv = new CsvReader(streamReader, configuration);

        if (!csv.Read() || !csv.ReadHeader())
            throw new InvalidDataException($"File '{path}' contains no header record.");

        var header = csv.HeaderRecord ?? [];
        EnsureColumn(header, path, DateColumn);
        EnsureColumn(header, path, valueColumn);

        while (csv.Read())
        {
            var date = csv.GetField(DateColumn) ?? string.Empty;
            var value = csv.GetField<decimal>(valueColumn);
            yield return new DataRow(date, value);
        }
    }

    private static void EnsureColumn(string[] header, string path, string column)
    {
        var exists = header.Any(h => string.Equals(h?.Trim(), column, StringComparison.OrdinalIgnoreCase));
        if (!exists)
        {
            throw new InvalidDataException($"File '{path}' has no '{column}' column.");
        }
    }
}
