namespace PriceAnomaly.Tests;

internal static class TestFiles
{
    public static string Resolve(string name)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", name);
    }
}
