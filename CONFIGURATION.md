# SeriesList Configuration Guide

## Overview
The SeriesListLib library has been updated to make hard-coded paths and patterns configurable. The main classes are now instantiable and require a `SeriesListConfiguration` parameter.

## Delegates

The configuration includes five delegate properties that allow you to customize behavior:

- **`GetPathSegmentForGrouping`** (`Func<string, string>`): Extracts a path segment for grouping seasons during parallel processing
- **`ExtractSeriesName`** (`Func<string, string>`): Extracts the series name from a season folder path
- **`ExtractSeasonName`** (`Func<string, string>`): Extracts the season name from a season folder path
- **`CleanPath`** (`Func<string, string>`): Cleans/normalizes paths (e.g., removes drive prefixes, converts path separators)
- **`Feedback`** (`Action<string>`): Receives progress messages during processing

All delegates are required and must be provided by the caller.

## Configuration Options

The `SeriesListConfiguration` class provides the following configurable properties:

### SeasonFolderPatterns
- **Type**: `IEnumerable<string>`
- **Required**: Yes
- **Description**: Array of season folder patterns to search for. Used by the `FolderGetter` class to identify season folders.
- **Example**: `["Season ", "Staffel "]` or `["Season ", "Staffel ", "Saison "]`

### ArticlesToSkip
- **Type**: `IEnumerable<string>`
- **Required**: Yes
- **Description**: Array of articles (definite and indefinite) to skip when comparing series titles for sorting. Article matching is case-insensitive and only applies to the first word of a title.
- **Example**: `["the", "a", "an", "der", "die", "das", "ein", "eine"]`
- **Impact**: 
  - "The Matrix" would be sorted as "Matrix"
  - "A Beautiful Mind" would be sorted as "Beautiful Mind"
  - "Der Untergang" would be sorted as "Untergang"
  - "Enter the Dragon" would remain unchanged (article not at start)
- **Used by**: `SeriesKey` class for title comparison and sorting. Automatically passed to `SeriesKey` constructor by `FolderGetter`.

### RootPaths
- **Type**: `IEnumerable<string>`
- **Required**: Yes
- **Description**: Array of root paths to search for TV show folders. These are the top-level directories that will be scanned for series and season folders.
- **Example**: `[@"N:\Drive1\TVShows\", @"N:\Drive2\TVShows\", @"N:\Drive3\TVShows\"]`
- **Used by**: `FolderGetter` class to determine which directories to scan

### MaxDegreeOfParallelism
- **Type**: `int`
- **Required**: Yes
- **Description**: Maximum degree of parallelism for parallel operations in `FolderGetter` and `LanguageHelper`.
- **Example**: `4` or `8`

### GetPathSegmentForGrouping
- **Type**: `Func<string, string>`
- **Required**: Yes
- **Description**: A delegate function that extracts a path segment from a full path for grouping seasons during parallel processing. This allows custom logic for how paths are grouped.
- **Signature**: `string GetPathSegmentForGrouping(string fullPath)`
- **Example**: 
  ```csharp
  // Group by drive name (index 1 in path split)
  GetPathSegmentForGrouping = (fullPath) => fullPath.Split('\\')[1]

  // Group by top-level folder
  GetPathSegmentForGrouping = (fullPath) => fullPath.Split('\\')[2]

  // Custom grouping logic
  GetPathSegmentForGrouping = (fullPath) => 
  {
      var parts = fullPath.Split('\\');
      return parts.Length > 1 ? parts[1] : fullPath;
  }
  ```
- **Used by**: `LanguageHelper` class for grouping seasons before parallel processing

### ExtractSeriesName
- **Type**: `Func<string, string>`
- **Required**: Yes
- **Description**: A delegate function that extracts the series name from a season folder path. This determines how series are identified and grouped.
- **Signature**: `string ExtractSeriesName(string fullPath)`
- **Example**:
  ```csharp
  // Extract parent folder as series name (default behavior)
  ExtractSeriesName = (fullPath) =>
  {
      var parts = fullPath.Split('\\');
      return parts[parts.Length - 2]; // "C:\Shows\Breaking Bad\Season 1" → "Breaking Bad"
  }

  // Use regex for custom extraction
  ExtractSeriesName = (fullPath) =>
  {
      var match = Regex.Match(fullPath, @"\\([^\\]+)\\Season");
      return match.Success ? match.Groups[1].Value : "Unknown";
  }

  // Extract from specific path position
  ExtractSeriesName = (fullPath) =>
  {
      var parts = fullPath.Split('\\');
      return parts.Length > 3 ? parts[3] : "Unknown"; // Always use 4th segment
  }
  ```
- **Used by**: `FolderGetter` class to create `SeriesKey` instances

### ExtractSeasonName
- **Type**: `Func<string, string>`
- **Required**: Yes
- **Description**: A delegate function that extracts the season name from a season folder path. This determines the season identifier.
- **Signature**: `string ExtractSeasonName(string fullPath)`
- **Example**:
  ```csharp
  // Extract folder name as season name (default behavior)
  ExtractSeasonName = (fullPath) =>
  {
      var parts = fullPath.Split('\\');
      return parts[parts.Length - 1]; // "C:\Shows\Breaking Bad\Season 1" → "Season 1"
  }

  // Extract just the season number
  ExtractSeasonName = (fullPath) =>
  {
      var match = Regex.Match(fullPath, @"Season (\d+)");
      return match.Success ? match.Groups[1].Value : Path.GetFileName(fullPath);
  }

  // Normalize season format
  ExtractSeasonName = (fullPath) =>
  {
      var folderName = Path.GetFileName(fullPath);
      var match = Regex.Match(folderName, @"(\d+)");
      return match.Success ? $"Season {match.Groups[1].Value}" : folderName;
  }
  ```
- **Used by**: `FolderGetter` class to create `SeriesValue` instances

### CleanPath
- **Type**: `Func<string, string>`
- **Required**: Yes
- **Description**: A delegate function that cleans/normalizes full paths. This allows custom logic for path transformation, making it easy to handle multiple drives or different path formats.
- **Signature**: `string CleanPath(string fullPath)`
- **Example**:
  ```csharp
  // Clean paths from a single drive
  CleanPath = (fullPath) =>
  {
      if (fullPath.StartsWith(@"N:\", StringComparison.InvariantCultureIgnoreCase))
      {
          return fullPath.Substring(2).Replace("\\", "/").TrimEnd('/') + "/";
      }
      return fullPath;
  }

  // Handle multiple drives dynamically
  CleanPath = (fullPath) =>
  {
      var match = Regex.Match(fullPath, @"^([A-Z]):\\", RegexOptions.IgnoreCase);
      if (match.Success)
      {
          return fullPath.Substring(2).Replace("\\", "/").TrimEnd('/') + "/";
      }
      return fullPath;
  }

  // No cleaning - return as-is
  CleanPath = (fullPath) => fullPath
  ```
- **Used by**: `Cleaner` class to normalize season paths

### Feedback
- **Type**: `Action<string>`
- **Required**: Yes
- **Description**: A delegate function for receiving feedback/progress messages during processing. Allows the caller to handle logging, console output, or UI updates.
- **Signature**: `void Feedback(string message)`
- **Example**:
  ```csharp
  // Simple console output
  Feedback = (message) => Console.WriteLine(message)

  // Thread-safe console output for parallel operations
  var threadLock = new object();
  Feedback = (message) =>
  {
      lock (threadLock)
      {
          Console.WriteLine(message);
      }
  }

  // Logging to a file or logger
  Feedback = (message) => logger.Info(message)

  // UI updates
  Feedback = (message) => progressLabel.Text = message
  ```
- **Used by**: `LanguageHelper` class to report progress while processing each season folder

## Usage Examples

### Basic Usage
```csharp
var configuration = new SeriesListConfiguration
{
    SeasonFolderPatterns = ["Season ", "Staffel "],
    ArticlesToSkip = ["the", "a", "an", "der"],
    RootPaths = [@"N:\Drive1\TVShows\", @"N:\Drive2\TVShows\"],
    MaxDegreeOfParallelism = 4,
    GetPathSegmentForGrouping = (fullPath) => fullPath.Split('\\')[1],
    ExtractSeriesName = (fullPath) =>
    {
        var parts = fullPath.Split('\\');
        return parts[parts.Length - 2]; // Parent folder
    },
    ExtractSeasonName = (fullPath) =>
    {
        var parts = fullPath.Split('\\');
        return parts[parts.Length - 1]; // Folder name
    },
    CleanPath = (fullPath) =>
    {
        if (fullPath.StartsWith(@"N:\", StringComparison.InvariantCultureIgnoreCase))
        {
            return fullPath.Substring(2).Replace("\\", "/").TrimEnd('/') + "/";
        }
        return fullPath;
    },
    Feedback = (message) => Console.WriteLine(message)
};

var folderGetter = new FolderGetter(configuration);
var languageHelper = new LanguageHelper(configuration);
var cleaner = new Cleaner(configuration);

// Get folders - ArticlesToSkip is automatically used when creating SeriesKey instances
var folders = folderGetter.Get();
```

### Handling Multiple Drives
```csharp
var threadLock = new object();

var configuration = new SeriesListConfiguration
{
    SeasonFolderPatterns = ["Season ", "Staffel ", "Saison "],
    ArticlesToSkip = ["the", "a", "an", "der", "die", "das", "le", "la", "les", "un", "une"],
    RootPaths = [@"N:\Drive1\TVShows\", @"M:\Media\Shows\", @"P:\Archive\TVSeries\"],
    MaxDegreeOfParallelism = 8,
    GetPathSegmentForGrouping = (fullPath) =>
    {
        var parts = fullPath.Split('\\');
        return parts.Length > 1 ? parts[1] : fullPath;
    },
    ExtractSeriesName = (fullPath) =>
    {
        var parts = fullPath.Split('\\');
        return parts[parts.Length - 2]; // Parent folder
    },
    ExtractSeasonName = (fullPath) =>
    {
        var parts = fullPath.Split('\\');
        return parts[parts.Length - 1]; // Folder name
    },
    CleanPath = (fullPath) =>
    {
        // Handle any Windows drive letter dynamically
        var match = Regex.Match(fullPath, @"^([A-Z]):\\", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return fullPath.Substring(2).Replace("\\", "/").TrimEnd('/') + "/";
        }
        return fullPath;
    },
    Feedback = (message) =>
    {
        lock (threadLock)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
        }
    }
};

var folderGetter = new FolderGetter(configuration);
var languageHelper = new LanguageHelper(configuration);
var cleaner = new Cleaner(configuration);

// Get folders - ArticlesToSkip is automatically used
var folders = folderGetter.Get();
```

### Sharing Configuration Across Instances
```csharp
// Create configuration once
var threadLock = new object();

var configuration = new SeriesListConfiguration
{
    SeasonFolderPatterns = ["Season ", "Staffel "],
    ArticlesToSkip = ["the", "a", "an", "der"],
    RootPaths = [@"N:\Drive1\TVShows\", @"N:\Drive2\TVShows\", @"N:\Drive3\TVShows\", @"N:\Drive4\TVShows\"],
    MaxDegreeOfParallelism = 4,
    GetPathSegmentForGrouping = (fullPath) => fullPath.Split('\\')[1],
    CleanPath = (fullPath) =>
    {
        if (fullPath.StartsWith(@"N:\", StringComparison.InvariantCultureIgnoreCase))
        {
            return fullPath.Substring(2).Replace("\\", "/").TrimEnd('/') + "/";
        }
        return fullPath;
    },
    Feedback = (message) =>
    {
        lock (threadLock)
        {
            Console.WriteLine(message);
        }
    }
};

// Use the same configuration for all instances
var folderGetter = new FolderGetter(configuration);
var folders = folderGetter.Get();

var rootItem = XmlCreator.Create(folders);

var languageHelper = new LanguageHelper(configuration);
languageHelper.EnrichLanguages(rootItem);

var cleaner = new Cleaner(configuration);
cleaner.Clean(rootItem);
```

## Modified Classes

### FolderGetter
- Now instantiable (was static)
- Constructor: `FolderGetter(SeriesListConfiguration configuration)` - **configuration is required**
- Method: `Get()` - no longer takes parameters; uses `RootPaths` from configuration
- Uses `SeasonFolderPatterns` to identify season folders
- Uses `MaxDegreeOfParallelism` for parallel folder processing
- Uses `RootPaths` to determine which directories to scan

### Cleaner
- Now instantiable (was static)
- Constructor: `Cleaner(SeriesListConfiguration configuration)` - **configuration is required**
- Uses `CleanPath` delegate to normalize paths

### LanguageHelper
- Now instantiable (was static)
- Constructor: `LanguageHelper(SeriesListConfiguration configuration)` - **configuration is required**
- Uses `MaxDegreeOfParallelism` for parallel language processing
- Uses `GetPathSegmentForGrouping` delegate to group seasons by custom path logic for efficient parallel processing
- Uses `Feedback` delegate to report progress for each season folder being processed

## Important Notes
- **Configuration is required**: All classes now require a `SeriesListConfiguration` instance. Passing `null` will throw an `ArgumentNullException`.
- **No default values in library**: Default values must be set by the caller in their application code.
- **mp3 and mp4**: File extensions remain hard-coded as requested.
- The configuration can be shared across multiple instances for consistency.

## Understanding GetPathSegmentForGrouping

The `GetPathSegmentForGrouping` delegate allows you to define custom logic for grouping seasons during parallel processing in `LanguageHelper`. This is useful for:

- **Optimizing parallel processing**: Group seasons by physical drive or network location to avoid I/O contention
- **Custom folder structures**: Adapt to any folder hierarchy
- **Performance tuning**: Group by any path segment that makes sense for your data organization

### Common Patterns

**Group by drive (default pattern):**
```csharp
GetPathSegmentForGrouping = (fullPath) => fullPath.Split('\\')[1]
// "N:\Drive1\TVShows\..." → "Drive1"
```

**Group by top-level folder:**
```csharp
GetPathSegmentForGrouping = (fullPath) => fullPath.Split('\\')[2]
// "N:\Drive1\TVShows\..." → "TVShows"
```

**Group with error handling:**
```csharp
GetPathSegmentForGrouping = (fullPath) =>
{
    var parts = fullPath.Split('\\');
    return parts.Length > 1 ? parts[1] : fullPath;
}
```

**Group by multiple criteria:**
```csharp
GetPathSegmentForGrouping = (fullPath) =>
{
    var parts = fullPath.Split('\\');
    return parts.Length > 2 ? $"{parts[1]}_{parts[2]}" : fullPath;
}
// "N:\Drive1\TVShows\..." → "Drive1_TVShows"
```

## Understanding Series and Season Name Extraction

The `ExtractSeriesName` and `ExtractSeasonName` delegates allow you to customize how series and season identifiers are extracted from folder paths. This is important because different folder structures require different extraction logic.

### Why Configurable Extraction?

Different organizational structures require different extraction logic:

**Standard Structure:**
```
N:\TVShows\Breaking Bad\Season 1
           └─ Series  └─ Season
```

**Nested Structure:**
```
N:\Media\Shows\Drama\Breaking Bad\Season 01
                     └─ Series  └─ Season
```

**Custom Structure:**
```
N:\Archive\BB_S01
           └─ Need to parse "BB" and "S01"
```

### Default Extraction (Recommended for Standard Structure)

```csharp
ExtractSeriesName = (fullPath) =>
{
    var parts = fullPath.Split('\\');
    return parts[parts.Length - 2]; // Parent folder = series name
};

ExtractSeasonName = (fullPath) =>
{
    var parts = fullPath.Split('\\');
    return parts[parts.Length - 1]; // Folder name = season name
};
```

This works for paths like: `N:\TVShows\Breaking Bad\Season 1`
- Series: "Breaking Bad"
- Season: "Season 1"

### Custom Extraction Examples

**Extract from specific path depth:**
```csharp
// For structure: N:\Media\Shows\Drama\Breaking Bad\Season 01
ExtractSeriesName = (fullPath) =>
{
    var parts = fullPath.Split('\\');
    return parts.Length > 4 ? parts[4] : "Unknown"; // Always use 5th segment
};
```

**Use regex for complex names:**
```csharp
// For abbreviated folders like "BB_S01", "GOT_S02"
ExtractSeriesName = (fullPath) =>
{
    var folderName = Path.GetFileName(fullPath);
    var match = Regex.Match(folderName, @"^([A-Z]+)_S\d+$");
    if (match.Success)
    {
        // Map abbreviations to full names
        return match.Groups[1].Value switch
        {
            "BB" => "Breaking Bad",
            "GOT" => "Game of Thrones",
            _ => match.Groups[1].Value
        };
    }
    return "Unknown";
};

ExtractSeasonName = (fullPath) =>
{
    var folderName = Path.GetFileName(fullPath);
    var match = Regex.Match(folderName, @"_S(\d+)$");
    return match.Success ? $"Season {match.Groups[1].Value}" : folderName;
};
```

**Normalize season numbering:**
```csharp
// Convert "Season 1", "Season 01", "S01" to consistent format
ExtractSeasonName = (fullPath) =>
{
    var folderName = Path.GetFileName(fullPath);
    var match = Regex.Match(folderName, @"(?:Season\s*|S)(\d+)", RegexOptions.IgnoreCase);
    if (match.Success)
    {
        var seasonNum = int.Parse(match.Groups[1].Value);
        return $"Season {seasonNum:D2}"; // Always "Season 01", "Season 02", etc.
    }
    return folderName;
};
```

**Handle multi-language folders:**
```csharp
// Handle "Season 1", "Staffel 1", "Saison 1"
ExtractSeasonName = (fullPath) =>
{
    var folderName = Path.GetFileName(fullPath);
    var patterns = new[] { @"Season\s*(\d+)", @"Staffel\s*(\d+)", @"Saison\s*(\d+)" };

    foreach (var pattern in patterns)
    {
        var match = Regex.Match(folderName, pattern, RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return $"Season {match.Groups[1].Value}"; // Normalize to English
        }
    }
    return folderName;
};
```

### Best Practices

1. **Always include error handling**: Return a sensible default if parsing fails
2. **Test with real paths**: Use actual folder structures from your library
3. **Keep it simple**: Complex logic can slow down scanning
4. **Document assumptions**: Comment what path structure you expect

## Understanding ArticlesToSkip

The `ArticlesToSkip` configuration allows you to specify which articles (definite and indefinite) should be ignored when sorting series titles. This is important for natural alphabetical sorting of TV series.

### Why Skip Articles?

Without skipping articles, series would be sorted like:
- A Team, The
- Breaking Bad
- Der Untergang
- The Matrix
- The Wire

With articles skipped, the sorting becomes more natural:
- Breaking Bad
- Matrix, The
- Team, The (A)
- Untergang, Der
- Wire, The

### How It Works

1. **Only the first word is checked**: Articles are only removed if they appear at the start of the title
2. **Case-insensitive matching**: "The", "THE", and "the" are all treated the same
3. **Language support**: Configure articles from any language
4. **Automatic application**: The `FolderGetter` automatically passes articles to `SeriesKey` instances when creating them

### Common Articles by Language

**English:**
```csharp
ArticlesToSkip = ["the", "a", "an"]
```

**German:**
```csharp
ArticlesToSkip = ["der", "die", "das", "ein", "eine"]
```

**French:**
```csharp
ArticlesToSkip = ["le", "la", "les", "un", "une", "l'"]
```

**Spanish:**
```csharp
ArticlesToSkip = ["el", "la", "los", "las", "un", "una"]
```

**Multi-language (recommended):**
```csharp
ArticlesToSkip = [
    // English
    "the", "a", "an",
    // German
    "der", "die", "das", "ein", "eine",
    // French
    "le", "la", "les", "un", "une",
    // Spanish
    "el", "la", "los", "las", "un", "una"
]
```

### Edge Cases

**Empty string or null:**
- Empty collection `[]` means no articles are skipped
- `null` is treated the same as an empty collection

**Single-word titles:**
- Titles with only one word won't be affected even if that word is an article
- "The" as a title remains "The"

**Articles in the middle:**
- "Enter the Dragon" → remains "Enter the Dragon" (article not at start)
- "The Man from U.N.C.L.E." → becomes "Man from U.N.C.L.E." (article at start)

## Working with Multiple Drives

When your `RootPaths` configuration includes paths from different Windows drives (e.g., `N:\`, `M:\`, `P:\`), the `CleanPath` delegate becomes especially important for proper path normalization.

### The Problem

If you have root paths like:
```csharp
RootPaths = [@"N:\Drive1\TVShows\", @"M:\Media\Shows\", @"P:\Archive\TVSeries\"]
```

A simple string-based prefix removal (like the old `DrivePrefix = @"N:\"`) would only work for the N: drive paths and leave M: and P: paths unchanged.

### The Solution

Use a `CleanPath` delegate that can handle any drive letter:

```csharp
CleanPath = (fullPath) =>
{
    // Match any drive letter at the start (e.g., N:\, M:\, P:\)
    var match = Regex.Match(fullPath, @"^([A-Z]):\\", RegexOptions.IgnoreCase);
    if (match.Success)
    {
        // Remove drive letter and colon, convert backslashes to forward slashes
        return fullPath.Substring(2).Replace("\\", "/").TrimEnd('/') + "/";
    }
    // Return unchanged if no drive letter found
    return fullPath;
}
```

### Alternative Approaches

**Option 1: Remove only specific drives**
```csharp
CleanPath = (fullPath) =>
{
    var prefixes = new[] { @"N:\", @"M:\", @"P:\" };
    foreach (var prefix in prefixes)
    {
        if (fullPath.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
        {
            return fullPath.Substring(2).Replace("\\", "/").TrimEnd('/') + "/";
        }
    }
    return fullPath;
}
```

**Option 2: No cleaning (keep original paths)**
```csharp
CleanPath = (fullPath) => fullPath
```

**Option 3: Custom transformation per drive**
```csharp
CleanPath = (fullPath) =>
{
    if (fullPath.StartsWith(@"N:\", StringComparison.InvariantCultureIgnoreCase))
    {
        return "/primary" + fullPath.Substring(2).Replace("\\", "/");
    }
    else if (fullPath.StartsWith(@"M:\", StringComparison.InvariantCultureIgnoreCase))
    {
        return "/secondary" + fullPath.Substring(2).Replace("\\", "/");
    }
    return fullPath;
}
```

### Best Practices

1. **Match your RootPaths**: Ensure your `CleanPath` delegate can handle all drives in your `RootPaths` configuration
2. **Test edge cases**: Consider UNC paths, relative paths, or network drives if applicable
3. **Document your logic**: Comment your cleaning logic to explain the transformation
4. **Be consistent**: All paths should follow the same normalization pattern

## Understanding Feedback Delegate

The `Feedback` delegate provides a way to receive progress notifications during processing. This is particularly important when using the `LanguageHelper` class, as it reports each season folder being processed.

### Why Use Feedback?

- **Progress tracking**: Monitor which folders are currently being processed
- **Debugging**: Identify slow or problematic folders
- **User experience**: Display progress to end users
- **Logging**: Record operations for audit or troubleshooting

### Thread Safety Considerations

Since `LanguageHelper` uses parallel processing, the `Feedback` delegate will be called from multiple threads simultaneously. **You must ensure your feedback implementation is thread-safe.**

**Thread-safe console output:**
```csharp
var threadLock = new object();
Feedback = (message) =>
{
    lock (threadLock)
    {
        Console.WriteLine(message);
    }
}
```

**Thread-safe file logging:**
```csharp
var logLock = new object();
Feedback = (message) =>
{
    lock (logLock)
    {
        File.AppendAllText("log.txt", $"{DateTime.Now}: {message}{Environment.NewLine}");
    }
}
```

**Using thread-safe loggers:**
```csharp
// Most logging frameworks are already thread-safe
Feedback = (message) => logger.Info(message)
```

**UI updates (must marshal to UI thread):**
```csharp
// WPF example
Feedback = (message) =>
{
    Application.Current.Dispatcher.Invoke(() =>
    {
        progressLabel.Content = message;
    });
}

// WinForms example
Feedback = (message) =>
{
    if (statusLabel.InvokeRequired)
    {
        statusLabel.Invoke(new Action(() => statusLabel.Text = message));
    }
    else
    {
        statusLabel.Text = message;
    }
}
```

## Migration from Static Classes
If you were using the old static API:
```csharp
// Old code
var folders = FolderGetter.Get(roots);
Cleaner.Clean(rootItem);
LanguageHelper.EnrichLanguages(rootItem);
```

Update to:
```csharp
// New code
var threadLock = new object();

var configuration = new SeriesListConfiguration
{
    SeasonFolderPatterns = ["Season ", "Staffel "],
    ArticlesToSkip = ["the", "a", "an", "der"],
    RootPaths = [@"N:\Drive1\TVShows\", @"N:\Drive2\TVShows\"],
    MaxDegreeOfParallelism = 4,
    GetPathSegmentForGrouping = (fullPath) => fullPath.Split('\\')[1],
    ExtractSeriesName = (fullPath) =>
    {
        var parts = fullPath.Split('\\');
        return parts[parts.Length - 2]; // Parent folder (matches old behavior)
    },
    ExtractSeasonName = (fullPath) =>
    {
        var parts = fullPath.Split('\\');
        return parts[parts.Length - 1]; // Folder name (matches old behavior)
    },
    CleanPath = (fullPath) =>
    {
        if (fullPath.StartsWith(@"N:\", StringComparison.InvariantCultureIgnoreCase))
        {
            return fullPath.Substring(2).Replace("\\", "/").TrimEnd('/') + "/";
        }
        return fullPath;
    },
    Feedback = (message) =>
    {
        lock (threadLock)
        {
            Console.WriteLine(message);
        }
    }
};

var folderGetter = new FolderGetter(configuration);
var folders = folderGetter.Get(); // No parameters needed - uses configuration
// ArticlesToSkip is automatically applied when SeriesKey instances are created

var cleaner = new Cleaner(configuration);
cleaner.Clean(rootItem);

var languageHelper = new LanguageHelper(configuration);
languageHelper.EnrichLanguages(rootItem);
```

### Note on Name Extraction Migration

The old code used hard-coded path splitting:
- Series name: `pathParts[pathParts.Length - 2]` (parent folder)
- Season name: `pathParts[pathParts.Length - 1]` (folder name)

To maintain the same behavior, use the extractors shown above.

To customize for different folder structures, see the "Understanding Series and Season Name Extraction" section.

### Note on ArticlesToSkip Migration

The old code had hard-coded articles: `"the"`, `"a"`, `"der"`

To maintain the same behavior, use:
```csharp
ArticlesToSkip = ["the", "a", "der"]
```

To add support for more articles:
```csharp
ArticlesToSkip = ["the", "a", "an", "der", "die", "das", "ein", "eine"]
```
