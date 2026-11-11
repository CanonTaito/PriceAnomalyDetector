using PriceAnomaly.Console;

namespace PriceAnomaly.Tests.Integration;

public class ProgramEndToEndTests : IDisposable
{
    private readonly TextWriter _originalOut;
    private readonly TextWriter _originalError;
    private readonly StringWriter _output = new();
    private readonly StringWriter _error = new();

    public ProgramEndToEndTests()
    {
        _originalOut = System.Console.Out;
        _originalError = System.Console.Error;
        System.Console.SetOut(_output);
        System.Console.SetError(_error);
    }

    public void Dispose()
    {
        System.Console.SetOut(_originalOut);
        System.Console.SetError(_originalError);
        _output.Dispose();
        _error.Dispose();
    }

    [Fact]
    public void Main_CommFixture_PrintsAbnormalValueLinesAndSucceeds()
    {
        var exitCode = Program.Main([TestFiles.Resolve("comm_sample.csv")]);

        Assert.Equal(0, exitCode);
        Assert.Equal($"comm_sample.csv 5/06/2021 130.00 105.75{Environment.NewLine}", _output.ToString());
        Assert.Empty(_error.ToString());
    }

    [Fact]
    public void Main_UnsupportedFile_WritesErrorAndReturnsNonZero()
    {
        var exitCode = Program.Main([TestFiles.Resolve("unsupported_sample.csv")]);

        Assert.Equal(1, exitCode);
        Assert.Empty(_output.ToString());
        Assert.Contains("unsupported_sample.csv", _error.ToString());
    }

    [Fact]
    public void Main_NoArguments_WritesUsageAndReturnsNonZero()
    {
        var exitCode = Program.Main([]);

        Assert.Equal(1, exitCode);
        Assert.Contains("Usage", _error.ToString());
    }
}
