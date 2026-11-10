using PriceAnomaly.Console.IO;
using PriceAnomaly.Console.Models;

namespace PriceAnomaly.Tests.IO;

public class CsvRowReaderTests
{
    private readonly CsvRowReader _reader = new();

    [Fact]
    public void Read_ReturnsDateAndValueRows()
    {
        var rows = _reader.Read(FixturePath("comm_sample.csv"), "Price SOD").ToList();

        var expected = new List<DataRow>
        {
            new("1/06/2021", 100m),
            new("2/06/2021", 110.5m),
            new("3/06/2021", 90.25m),
            new("4/06/2021", 105.75m),
            new("5/06/2021", 130m),
        };
        Assert.Equal(expected, rows);
    }

    [Fact]
    public void Read_MissingValueColumn_ThrowsInvalidData()
    {
        var exception = Assert.Throws<InvalidDataException>(
            () => _reader.Read(FixturePath("missing_value_column.csv"), "Price SOD").ToList());

        Assert.Contains("Price SOD", exception.Message);
    }

    [Fact]
    public void Read_HeaderOnly_ReturnsEmpty()
    {
        var rows = _reader.Read(FixturePath("header_only.csv"), "Price SOD").ToList();

        Assert.Empty(rows);
    }

    [Fact]
    public void Read_EmptyFile_ThrowsInvalidData()
    {
        Assert.Throws<InvalidDataException>(
            () => _reader.Read(FixturePath("empty.csv"), "Price SOD").ToList());
    }

    private static string FixturePath(string name)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", name);
    }
}
