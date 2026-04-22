namespace DoenaSoft.SeriesList.Configuration;

/// <summary>
/// Configuration options for SeriesList library operations.
/// </summary>
public class SeriesListConfiguration
{
    /// <summary>
    /// Gets or sets the season folder patterns to search for (e.g., "Season ", "Staffel ").
    /// </summary>
    public IEnumerable<string> SeasonFolderPatterns { get; set; }

    /// <summary>
    /// Gets or sets the articles to skip when comparing series titles.
    /// These are typically definite and indefinite articles that should be ignored for sorting purposes.
    /// </summary>
    /// <example>
    /// <code>
    /// ArticlesToSkip = ["the", "a", "an", "der", "die", "das", "ein", "eine"]
    /// </code>
    /// </example>
    /// <remarks>
    /// Article matching is case-insensitive and only applies to the first word of a title.
    /// For example, "The Matrix" would be sorted as "Matrix", but "Enter the Dragon" would remain unchanged.
    /// </remarks>
    public IEnumerable<string> ArticlesToSkip { get; set; }

    /// <summary>
    /// Gets or sets the root paths to search for TV show folders.
    /// These are the top-level directories that will be scanned for series and season folders.
    /// </summary>
    /// <example>
    /// <code>
    /// RootPaths = [@"N:\Drive1\TVShows\", @"N:\Drive2\TVShows\", @"N:\Drive3\TVShows\"]
    /// </code>
    /// </example>
    public IEnumerable<string> RootPaths { get; set; }

    /// <summary>
    /// Gets or sets the maximum degree of parallelism for folder processing.
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; }

    /// <summary>
    /// Gets or sets the delegate function used to extract a path segment for grouping seasons.
    /// This function takes a full path and returns a string segment that will be used to group
    /// seasons for parallel processing in LanguageHelper.
    /// </summary>
    public Func<string, string> GetPathSegmentForGrouping { get; set; }

    /// <summary>
    /// Gets or sets the delegate function used to extract the series name from a season folder path.
    /// This function takes the full path to a season folder and returns the series name.
    /// </summary>
    /// <example>
    /// Extract from parent folder (default behavior):
    /// <code>
    /// ExtractSeriesName = (fullPath) =>
    /// {
    ///     var parts = fullPath.Split('\\');
    ///     return parts[parts.Length - 2]; // Parent folder
    /// };
    /// </code>
    /// Custom extraction:
    /// <code>
    /// ExtractSeriesName = (fullPath) =>
    /// {
    ///     // Extract from a specific position or use regex
    ///     var match = Regex.Match(fullPath, @"\\([^\\]+)\\Season");
    ///     return match.Success ? match.Groups[1].Value : "Unknown";
    /// };
    /// </code>
    /// </example>
    public Func<string, string> ExtractSeriesName { get; set; }

    /// <summary>
    /// Gets or sets the delegate function used to extract the season name from a season folder path.
    /// This function takes the full path to a season folder and returns the season name.
    /// </summary>
    /// <example>
    /// Extract folder name (default behavior):
    /// <code>
    /// ExtractSeasonName = (fullPath) =>
    /// {
    ///     var parts = fullPath.Split('\\');
    ///     return parts[parts.Length - 1]; // Folder name
    /// };
    /// </code>
    /// Custom extraction:
    /// <code>
    /// ExtractSeasonName = (fullPath) =>
    /// {
    ///     // Extract just the number from "Season 01" format
    ///     var match = Regex.Match(fullPath, @"Season (\d+)");
    ///     return match.Success ? match.Groups[1].Value : Path.GetFileName(fullPath);
    /// };
    /// </code>
    /// </example>
    public Func<string, string> ExtractSeasonName { get; set; }

    /// <summary>
    /// Gets or sets the delegate function used to clean/normalize paths.
    /// This function takes a full path and returns a cleaned/normalized version.
    /// </summary>
    /// <remarks>
    /// Used by the Cleaner class to normalize paths. Common uses include:
    /// - Removing drive prefixes (e.g., "N:\" → "/")
    /// - Converting backslashes to forward slashes
    /// - Making paths relative
    /// When working with multiple drives in RootPaths, this delegate can handle different prefixes dynamically.
    /// </remarks>
    /// <example>
    /// Clean paths from N: drive:
    /// <code>
    /// CleanPath = (fullPath) =>
    /// {
    ///     if (fullPath.StartsWith(@"N:\", StringComparison.InvariantCultureIgnoreCase))
    ///     {
    ///         return fullPath.Substring(2).Replace("\\", "/").TrimEnd('/') + "/";
    ///     }
    ///     return fullPath;
    /// };
    /// </code>
    /// Handle multiple drives:
    /// <code>
    /// CleanPath = (fullPath) =>
    /// {
    ///     var match = Regex.Match(fullPath, @"^([A-Z]):\\", RegexOptions.IgnoreCase);
    ///     if (match.Success)
    ///     {
    ///         return fullPath.Substring(2).Replace("\\", "/").TrimEnd('/') + "/";
    ///     }
    ///     return fullPath;
    /// };
    /// </code>
    /// </example>
    public Func<string, string> CleanPath { get; set; }

    /// <summary>
    /// Gets or sets the delegate function used to receive feedback messages during processing.
    /// This function is called to report progress, typically with folder paths being processed.
    /// </summary>
    /// <remarks>
    /// This delegate is called from multiple threads during parallel processing in <see cref="LanguageHelper"/>.
    /// The implementation must be thread-safe. Consider using locks or thread-safe logging mechanisms.
    /// </remarks>
    /// <example>
    /// Thread-safe console output:
    /// <code>
    /// var threadLock = new object();
    /// configuration.Feedback = (message) =>
    /// {
    ///     lock (threadLock)
    ///     {
    ///         Console.WriteLine(message);
    ///     }
    /// };
    /// </code>
    /// </example>
    public Action<string> Feedback { get; set; }
}