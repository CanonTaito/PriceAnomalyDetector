namespace PriceAnomaly.Console.FileTypes;

public sealed class FileTypeResolver
{
    public static IReadOnlyList<FileTypeDefinition> DefaultDefinitions { get; } =
    [
        new FileTypeDefinition("COMM", "comm", "Price SOD"),
        new FileTypeDefinition("MOD", "mod", "MOD Duration"),
    ];

    private readonly IReadOnlyList<FileTypeDefinition> _definitions;

    public FileTypeResolver()
        : this(DefaultDefinitions)
    {
    }

    public FileTypeResolver(IEnumerable<FileTypeDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        _definitions = [.. definitions];
    }

    public FileTypeDefinition Resolve(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var name = Path.GetFileName(fileName);
        var definition = _definitions.FirstOrDefault(d =>
            name.StartsWith(d.FileNamePrefix, StringComparison.OrdinalIgnoreCase));

        if (definition is null)
        {
            var supported = string.Join(", ", _definitions.Select(d => d.Name));
            throw new NotSupportedException(
                $"File '{name}' is not a supported file type. Supported types: {supported}.");
        }

        return definition;
    }
}
