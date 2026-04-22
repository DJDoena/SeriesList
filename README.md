# SeriesList

A personal .NET library for cataloging TV series folders on my NAS storage.

## What It Does

Scans TV show directories on my NAS, extracts series and season information, and creates an XML catalog with metadata. Built to handle my specific folder structure and multi-language series collection.

## Requirements

- .NET Framework 4.8.1
- Windows

## Basic Usage

```csharp
using DoenaSoft.SeriesList;
using DoenaSoft.SeriesList.Configuration;

var configuration = new SeriesListConfiguration
{
    SeasonFolderPatterns = ["Season ", "Staffel "],
    ArticlesToSkip = ["the", "a", "an", "der", "die", "das"],
    RootPaths = [@"N:\Drive1\TVShows\", @"N:\Drive2\TVShows\"],
    MaxDegreeOfParallelism = 4,

    GetPathSegmentForGrouping = (fullPath) => fullPath.Split('\\')[1],
    ExtractSeriesName = (fullPath) =>
    {
        var parts = fullPath.Split('\\');
        return parts[parts.Length - 2];
    },
    ExtractSeasonName = (fullPath) =>
    {
        var parts = fullPath.Split('\\');
        return parts[parts.Length - 1];
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
var folders = folderGetter.Get();

var rootItem = XmlCreator.Create(folders);

var languageHelper = new LanguageHelper(configuration);
languageHelper.EnrichLanguages(rootItem);

var cleaner = new Cleaner(configuration);
cleaner.Clean(rootItem);

Serializer.Serialize(rootItem, @"N:\TVShows\SeriesList.xml");
```

## Configuration

See [CONFIGURATION.md](CONFIGURATION.md) for detailed documentation on all configuration options and delegates.

## Key Features

- Configurable season folder patterns (multi-language support)
- Article skipping for natural sorting ("The Matrix" → "Matrix")
- Custom path extraction using delegates
- Parallel processing for large libraries
- Language metadata extraction from XML files
- Flexible path normalization

## Project Structure

```
SeriesListLib/          # Core library
SeriesList/             # Console application
CONFIGURATION.md        # Detailed configuration guide
```

## Author

DJDoena

