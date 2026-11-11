using PriceAnomaly.Console.FileTypes;
using PriceAnomaly.Console.IO;
using PriceAnomaly.Console.Models;
using PriceAnomaly.Console.Processing;

namespace PriceAnomaly.Tests.Processing;

public class FileAnomalyAnalyzerTests
{
    private readonly FileAnomalyAnalyzer _analyzer = new(new CsvRowReader(), new FileTypeResolver());

    [Fact]
    public void Analyze_CommFile_ReturnsAbnormalValues()
    {
        var result = _analyzer.Analyze(TestFiles.Resolve("comm_sample.csv"));

        var expected = new[] { new AbnormalValue("comm_sample.csv", "5/06/2021", 130m, 105.75m) };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Analyze_ModFile_ReturnsAbnormalValues()
    {
        var result = _analyzer.Analyze(TestFiles.Resolve("mod_sample.csv"));

        var expected = new[] { new AbnormalValue("mod_sample.csv", "5/06/2021", 0.030m, 0.0195m) };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Analyze_HeaderOnlyFile_ReturnsNoAbnormalValues()
    {
        var result = _analyzer.Analyze(TestFiles.Resolve("comm_header_only.csv"));

        Assert.Empty(result);
    }

    [Fact]
    public void Analyze_UnsupportedFileType_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(
            () => _analyzer.Analyze(TestFiles.Resolve("unsupported_sample.csv")));
    }
}
