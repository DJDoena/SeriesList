namespace DoenaSoft.SeriesList.Configuration;

/// <summary>
/// Delegate for extracting a path segment from a full path for grouping purposes.
/// </summary>
/// <param name="fullPath">The full path to extract a segment from.</param>
/// <returns>A string representing the path segment to use for grouping.</returns>
public delegate string PathSegmentExtractorDelegate(string fullPath);

/// <summary>
/// Delegate for receiving feedback messages during processing operations.
/// </summary>
/// <param name="message">The feedback message to be displayed, logged, or processed.</param>
/// <remarks>
/// This delegate is called from multiple threads during parallel processing.
/// Implementations must be thread-safe.
/// </remarks>
public delegate void FeedbackDelegate(string message);

/// <summary>
/// Delegate for cleaning/normalizing a full path.
/// </summary>
/// <param name="fullPath">The full path to clean/normalize.</param>
/// <returns>The cleaned/normalized path, or the original path if no cleaning is needed.</returns>
/// <remarks>
/// This delegate is used by the Cleaner class to normalize paths.
/// Common uses include removing drive prefixes and converting path separators.
/// </remarks>
public delegate string PathCleanerDelegate(string fullPath);

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
    public PathSegmentExtractorDelegate GetPathSegmentForGrouping { get; set; }

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
    public PathCleanerDelegate CleanPath { get; set; }

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
    public FeedbackDelegate Feedback { get; set; }
}