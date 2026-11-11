using CsvHelper;
using PriceAnomaly.Console.FileTypes;
using PriceAnomaly.Console.IO;
using PriceAnomaly.Console.Processing;
using PriceAnomaly.Console.Reporting;

namespace PriceAnomaly.Console;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            WriteUsage();
            return 1;
        }

        var files = ResolveInputFiles(args);
        if (files.Count == 0)
        {
            System.Console.Error.WriteLine("No CSV files found for the given arguments.");
            return 1;
        }

        var analyzer = new FileAnomalyAnalyzer(new CsvRowReader(), new FileTypeResolver());
        var writer = new AnomalyWriter(System.Console.Out);
        var failures = 0;

        foreach (var file in files)
        {
            try
            {
                var abnormalities = analyzer.Analyze(file);
                writer.Write(abnormalities);
            }
            catch (Exception ex) when (ex is NotSupportedException
                or InvalidDataException
                or CsvHelperException
                or IOException
                or UnauthorizedAccessException)
            {
                System.Console.Error.WriteLine($"Error processing '{file}': {ex.Message}");
                failures++;
            }
        }

        return failures == 0 ? 0 : 1;
    }

    private static void WriteUsage()
    {
        System.Console.Error.WriteLine("Usage: PriceAnomaly.Console <file|directory|pattern> [file|directory|pattern ...]");
        System.Console.Error.WriteLine("Examples:");
        System.Console.Error.WriteLine("  PriceAnomaly.Console data/comm_01.csv");
        System.Console.Error.WriteLine("  PriceAnomaly.Console data");
        System.Console.Error.WriteLine("  PriceAnomaly.Console \"data/*.csv\"");
    }

    private static IReadOnlyList<string> ResolveInputFiles(string[] args)
    {
        var files = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var arg in args)
        {
            if (arg.Contains('*') || arg.Contains('?'))
                AddGlobMatches(files, arg);
            else if (Directory.Exists(arg))
                AddFiles(files, arg, "*.csv");
            else if (File.Exists(arg))
                files.Add(Path.GetFullPath(arg));
            else
                System.Console.Error.WriteLine($"Warning: '{arg}' was not found and will be skipped.");
        }

        return [.. files];
    }

    private static void AddGlobMatches(ISet<string> files, string pattern)
    {
        var directory = Path.GetDirectoryName(pattern);
        if (string.IsNullOrEmpty(directory))
            directory = ".";

        if (!Directory.Exists(directory))
        {
            System.Console.Error.WriteLine($"Warning: directory '{directory}' was not found and will be skipped.");
            return;
        }

        AddFiles(files, directory, Path.GetFileName(pattern));
    }

    private static void AddFiles(ISet<string> files, string directory, string searchPattern)
    {
        foreach (var file in Directory.GetFiles(directory, searchPattern))
            files.Add(Path.GetFullPath(file));
    }
}
