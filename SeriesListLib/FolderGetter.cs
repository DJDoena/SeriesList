using System.Collections.Concurrent;

namespace DoenaSoft.SeriesList;

public static class FolderGetter
{
    public static Dictionary<SeriesKey, List<SeriesValue>> Get(params string[] roots)
    {
        var folderInfosLists = new BlockingCollection<List<KeyValuePair<SeriesKey, SeriesValue>>>();
        Parallel.ForEach(roots, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, root =>
        {
            var subResult = TryGet(root);

            folderInfosLists.Add(subResult);
        });

        var folderInfos = folderInfosLists
            .SelectMany(fi => fi)
            .ToList();

        var folders = new Dictionary<SeriesKey, List<SeriesValue>>(folderInfos.Count);

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

    private static List<KeyValuePair<SeriesKey, SeriesValue>> TryGet(string root)
        => Directory.Exists(root)
            ? [.. ExecuteGet(root)]
            : new(0);

    private static IEnumerable<KeyValuePair<SeriesKey, SeriesValue>> ExecuteGet(string root)
    {
        var folderNames = Directory.GetDirectories(root, "*.*", SearchOption.AllDirectories);

        foreach (var folderName in folderNames)
        {
            if (!folderName.Contains("Season ") && !folderName.Contains("Staffel "))
            {
                continue;
            }
            else if (!EndsWithNumber(folderName))
            {
                continue;
            }

            var pathParts = folderName.Split('\\');

            var key = new SeriesKey(pathParts[pathParts.Length - 2]);

            var value = new SeriesValue(pathParts[pathParts.Length - 1], folderName);

            yield return new(key, value);
        }
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