using System.Globalization;
using PriceAnomaly.Console.Models;
using PriceAnomaly.Console.Reporting;

namespace PriceAnomaly.Tests.Reporting;

public class AnomalyWriterTests
{
    [Fact]
    public void Write_FormatsAbnormalValuesAsSpaceSeparatedLines()
    {
        var stringWriter = new StringWriter();
        var writer = new AnomalyWriter(stringWriter);

        var abnormalities = new[]
        {
            new AbnormalValue("comm_01.csv", "5/07/2021", 22.8m, 19.0m),
            new AbnormalValue("comm_01.csv", "9/07/2021", 15.1m, 19.0m),
        };

        writer.Write(abnormalities);

        var expected =
            "comm_01.csv 5/07/2021 22.8 19.0" + Environment.NewLine +
            "comm_01.csv 9/07/2021 15.1 19.0" + Environment.NewLine;
        Assert.Equal(expected, stringWriter.ToString());
    }

    [Fact]
    public void Write_NonInvariantCulture_StillUsesDotDecimalSeparator()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        try
        {
            var stringWriter = new StringWriter();
            var writer = new AnomalyWriter(stringWriter);

            writer.Write([new AbnormalValue("comm_01.csv", "5/07/2021", 22.8m, 19.05m)]);

            Assert.Equal("comm_01.csv 5/07/2021 22.8 19.05" + Environment.NewLine, stringWriter.ToString());
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void Write_NoAbnormalValues_WritesNothing()
    {
        var stringWriter = new StringWriter();
        var writer = new AnomalyWriter(stringWriter);

        writer.Write([]);

        Assert.Empty(stringWriter.ToString());
    }
}
