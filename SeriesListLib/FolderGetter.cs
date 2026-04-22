using DoenaSoft.SeriesList.Configuration;
using DoenaSoft.SeriesList.DataObjects;
using System.Collections.Concurrent;

namespace DoenaSoft.SeriesList;

public sealed class FolderGetter
{
    private readonly SeriesListConfiguration _configuration;

    public FolderGetter(SeriesListConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public ISeriesDictionary Get()
    {
        var folderInfosLists = new BlockingCollection<IEnumerable<KeyValuePair<SeriesKey, SeriesValue>>>();

        Parallel.ForEach(_configuration.RootPaths, new ParallelOptions() { MaxDegreeOfParallelism = _configuration.MaxDegreeOfParallelism }, root =>
        {
            var subResult = this.TryGet(root);

            folderInfosLists.Add(subResult);
        });

        var folderInfos = folderInfosLists
            .SelectMany(fi => fi)
            .ToList();

        var folders = new SeriesDictionary(folderInfos.Count);

        foreach (var folderInfo in folderInfos)
        {
            if (!folders.TryGetValue(folderInfo.Key, out var value))
            {
                value = [];

                folders.Add(folderInfo.Key, value);
            }

            value.Add(folderInfo.Value);
        }

        return folders;
    }

    private IEnumerable<KeyValuePair<SeriesKey, SeriesValue>> TryGet(string root)
        => Directory.Exists(root)
            ? [.. this.ExecuteGet(root)]
            : [];

    private IEnumerable<KeyValuePair<SeriesKey, SeriesValue>> ExecuteGet(string root)
    {
        var folderNames = Directory.GetDirectories(root, "*.*", SearchOption.AllDirectories);

        foreach (var folderName in folderNames)
        {
            if (!this.ContainsSeasonPattern(folderName))
            {
                continue;
            }
            else if (!EndsWithNumber(folderName))
            {
                continue;
            }

            var seriesName = _configuration.ExtractSeriesName(folderName);

            var key = new SeriesKey(seriesName, _configuration.ArticlesToSkip);

            var seasonName = _configuration.ExtractSeasonName(folderName);

            var value = new SeriesValue(seasonName, folderName);

            yield return new(key, value);
        }
    }

    private bool ContainsSeasonPattern(string folderName)
    {
        foreach (var pattern in _configuration.SeasonFolderPatterns)
        {
            if (folderName.Contains(pattern))
            {
                return true;
            }
        }

        return false;
    }

    private static bool EndsWithNumber(string folder)
    {
        var endsWithNumber = folder.EndsWith("0")
            || folder.EndsWith("1")
            || folder.EndsWith("2")
            || folder.EndsWith("3")
            || folder.EndsWith("4")
            || folder.EndsWith("5")
            || folder.EndsWith("6")
            || folder.EndsWith("7")
            || folder.EndsWith("8")
            || folder.EndsWith("9");

        if (endsWithNumber && (folder.EndsWith("mp4") || folder.EndsWith("mp3")))
        {
            endsWithNumber = false;
        }

        return endsWithNumber;
    }
}