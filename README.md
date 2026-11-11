# Price Anomaly Detector

A .NET 10 console program that reads "COMM" and "MOD" CSV files, calculates the median of the relevant column for each file, and prints every value that is more than 20% above or below that median.

| File type | Value column   | Date column |
|-----------|----------------|-------------|
| COMM      | `Price SOD`    | `Date`      |
| MOD       | `MOD Duration` | `Date`      |

## Running it

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run --project src/PriceAnomaly.Console -- data
```

Arguments can be any mix of files, directories (all `*.csv` inside) and wildcard patterns:

```bash
dotnet run --project src/PriceAnomaly.Console -- data/comm_01.csv "data/mod_*.csv"
```

Output is one line per abnormal value, in the format `{file name} {date} {value} {median value}`:

```
comm_01.csv 21/07/2021 25.2698 17.4398131
comm_01.csv 22/07/2021 27.3648996 17.4398131
comm_02.csv 15/08/2021 129.245 100.7740867
```

Results go to standard output; warnings and per-file errors go to standard error, so the output can be piped cleanly. A file that fails (unknown type, missing column, unparseable value) is reported and skipped, the remaining files are still processed, and the exit code is `1`.

## Running the tests

```bash
dotnet test
```

## Structure

```
src/PriceAnomaly.Console
  Program.cs                 Composition root: argument handling, wiring, error reporting
  FileTypes/                 FileTypeDefinition (name, file prefix, value column) and its resolver
  IO/CsvRowReader.cs         Streams (Date, Value) rows from a CSV using CsvHelper
  Analysis/                  MedianCalculator and ThresholdAnomalyDetector - pure functions, no I/O
  Processing/                FileAnomalyAnalyzer - orchestrates resolve -> read -> median -> detect
  Reporting/AnomalyWriter.cs Formats results to any TextWriter
  Models/                    DataRow and AbnormalValue records
tests/PriceAnomaly.Tests     xUnit tests mirroring the folders above, plus end-to-end tests
data/                        The sample files supplied with the task
```

Each class has one reason to change: reading, calculating, detecting and writing are separate, so the calculation code is tested without touching the file system and the writer is tested against a `StringWriter`.

## Assumptions

The task leaves a few details open. These are the choices I made:

- **File type is identified by file name prefix** (`comm*`, `mod*`, case-insensitive), matching the sample files. A file with an unrecognised name is reported as an error rather than guessed at.
- **One median per file**, across every row, as the task states. `mod_03.csv` contains several assets in one file; I did not group by asset because the task does not ask for it, but it is the first thing I would confirm with the business.
- **"20% above or below" is exclusive.** A value exactly 20% from the median is not reported.
- **A median of zero** (as in `mod_03.csv`) makes a percentage band meaningless, so any non-zero value is reported and zero values are not.
- **Dates are printed exactly as they appear in the file.** They are not parsed, which avoids day/month ambiguity and keeps the output traceable back to the source row.
- **Values use `decimal`**, not `double`, so prices print exactly as supplied with no binary floating-point noise.
- **A row with a blank or non-numeric value fails the whole file.** For financial data I would rather stop and report a bad file than silently calculate a median from partial data.
- **Number formatting is culture-invariant** for both reading and writing, so results are the same on any machine.

## Things to consider

### 1. How could the program structure make it easy to add new file types?

A file type is data, not code: a `FileTypeDefinition` holds a name, a file name prefix and the value column. Adding a type that follows the same rule (a median over one numeric column) is one line in `FileTypeResolver.DefaultDefinitions`, or a list passed into the resolver's constructor, and nothing else changes. There is a test that adds a hypothetical "FX" type this way.

Next steps if the types diverged further:

- Load the definitions from configuration (`appsettings.json`) so that a new type needs no deployment.
- If a type needs different behaviour rather than a different column (a different date column, another statistic, a grouping key, a different threshold), move those into the definition, or introduce an `IFileTypeHandler` strategy per type that is registered with dependency injection. I held back from adding those interfaces now because with two types that differ only by column name they would be abstraction without a use.
- Identify the type from the header row rather than the file name if naming conventions cannot be relied on.

### 2. How could we process 10 million files quickly?

Each file is independent, so the problem is embarrassingly parallel. The current program processes files one at a time, which is right for a handful of files but not for millions. In order of return on effort:

- **Parallelise within one process.** Replace the `foreach` with `Parallel.ForEachAsync` and a bounded degree of parallelism. `FileAnomalyAnalyzer` holds no mutable state, so it is already safe to share across threads.
- **Enumerate lazily.** Use `Directory.EnumerateFiles` and feed a bounded `Channel` instead of building and sorting the full list of 10 million paths up front.
- **Use a single writer.** Workers push results to a channel and one consumer writes them, so output lines from different files do not interleave and the console is not a lock everyone contends on. At this scale the output should go to a file or a database rather than a console.
- **Measure before tuning.** The limit will be storage throughput rather than CPU, so the degree of parallelism should be tuned against the actual disk or network share.
- **Scale out.** Past one machine, partition the file list and run many workers - for example files in blob storage, a queue message per file or per batch, and containerised workers or Azure Functions that scale on queue length. Work must be idempotent and record which files are done, so a failed run can resume rather than restart.
- **Avoid rework.** If the files arrive over time, process them on arrival (event-driven) and skip any file whose content hash has already been processed.

### 3. What happens if a file is huge (1 GB or more)?

Reading is already streamed: `CsvRowReader` yields one row at a time and never loads the file text into memory. The limit is `FileAnomalyAnalyzer`, which materialises every row into a list because an exact median needs all the values and the rows are then walked a second time. A 1 GB file is tens of millions of rows, so memory use would be several gigabytes and the program would slow badly or run out of memory.

Options, simplest first:

- **Two passes over the file.** Pass one reads only the numeric values into an array (16 bytes per `decimal`, so roughly 16 MB per million rows) and finds the median. Pass two streams the file again and prints abnormal rows as they are found. Dates and other strings are never held in memory. This alone handles files of several gigabytes.
- **Selection instead of sorting.** Finding the median with quickselect is O(n) on average rather than the O(n log n) of a full sort.
- **When the values themselves do not fit in memory**, use an external sort, or accept an approximate median from a streaming sketch such as t-digest, which uses a fixed, small amount of memory. Whether an approximation is acceptable is a business decision rather than a technical one.
- **Push it to a tool built for it.** For data of this size on a regular basis, loading into a database or a columnar engine (DuckDB, Spark, a warehouse) and calculating the median there is likely to be the better answer than custom code.

## With more time

- Continuous integration (build and test on every push).
- Structured logging in place of writing errors directly to standard error.
- Options for the threshold and for the output destination.
