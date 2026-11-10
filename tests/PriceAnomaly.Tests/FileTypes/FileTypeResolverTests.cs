using PriceAnomaly.Console.FileTypes;

namespace PriceAnomaly.Tests.FileTypes;

public class FileTypeResolverTests
{
    private readonly FileTypeResolver _resolver = new();

    [Fact]
    public void Resolve_CommFileName_ReturnsPriceSodDefinition()
    {
        var definition = _resolver.Resolve("comm_01.csv");

        Assert.Equal("COMM", definition.Name);
        Assert.Equal("Price SOD", definition.ValueColumn);
    }

    [Fact]
    public void Resolve_ModFileName_ReturnsModDurationDefinition()
    {
        var definition = _resolver.Resolve("mod_01.csv");

        Assert.Equal("MOD", definition.Name);
        Assert.Equal("MOD Duration", definition.ValueColumn);
    }

    [Fact]
    public void Resolve_FullPathWithDifferentCasing_ResolvesDefinition()
    {
        var definition = _resolver.Resolve(@"C:\data\COMM_01.CSV");

        Assert.Equal("COMM", definition.Name);
        Assert.Equal("Price SOD", definition.ValueColumn);
    }

    [Fact]
    public void Resolve_UnknownFileName_ThrowsNotSupportedException()
    {
        var exception = Assert.Throws<NotSupportedException>(() => _resolver.Resolve("prices_01.csv"));

        Assert.Contains("prices_01.csv", exception.Message);
        Assert.Contains("COMM", exception.Message);
        Assert.Contains("MOD", exception.Message);
    }
}
