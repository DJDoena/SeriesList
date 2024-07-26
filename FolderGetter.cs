namespace DoenaSoft.SeriesList;

internal static class FolderGetter
{
    internal static Dictionary<SeriesKey, List<SeriesValue>> Get()
    {
        var roots = new[] { @"N:\Drive1\TVShows\", @"N:\Drive2\TVShows\", @"N:\Drive3\TVShows\", @"N:\Drive4\TVShows\" };

        var tasks = new List<Task<List<KeyValuePair<SeriesKey, SeriesValue>>>>();

        foreach (var root in roots)
        {
            tasks.Add(Task.Run(() => TryGet(root)));
        }

        Task.WaitAll(tasks.ToArray());

        var folderInfos = tasks
            .SelectMany(t => t.Result)
            .ToList();

        var folders = new Dictionary<SeriesKey, List<SeriesValue>>(folderInfos.Count);

        foreach (var folderInfo in folderInfos)
        {
            if (!folders.TryGetValue(folderInfo.Key, out var value))
            {
                value = new List<SeriesValue>();

                folders.Add(folderInfo.Key, value);
            }

            value.Add(folderInfo.Value);
        }

        return folders;
    }

    private static List<KeyValuePair<SeriesKey, SeriesValue>> TryGet(string root)
        => Directory.Exists(root)
            ? ExecuteGet(root).ToList()
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
        var endsWithNumber = folder.EndsWith("0") || folder.EndsWith("1") || folder.EndsWith("2") || folder.EndsWith("3") || folder.EndsWith("4")
            || folder.EndsWith("5") || folder.EndsWith("6") || folder.EndsWith("7") || folder.EndsWith("8") || folder.EndsWith("9");

        if (endsWithNumber && (folder.EndsWith("mp4") || folder.EndsWith("mp3")))
        {
            endsWithNumber = false;
        }

        return endsWithNumber;
    }
}