namespace PriceAnomaly.Console.FileTypes;

public sealed class FileTypeResolver
{
    private static readonly IReadOnlyList<FileTypeDefinition> Definitions =
    [
        new FileTypeDefinition("COMM", "comm", "Price SOD"),
        new FileTypeDefinition("MOD", "mod", "MOD Duration"),
    ];

    public FileTypeDefinition Resolve(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var name = Path.GetFileName(fileName);
        var definition = Definitions.FirstOrDefault(d =>
            name.StartsWith(d.FileNamePrefix, StringComparison.OrdinalIgnoreCase));

        if (definition is null)
        {
            var supported = string.Join(", ", Definitions.Select(d => d.Name));
            throw new NotSupportedException(
                $"File '{name}' is not a supported file type. Supported types: {supported}.");
        }

        return definition;
    }
}
