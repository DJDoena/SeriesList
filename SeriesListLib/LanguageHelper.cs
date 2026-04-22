using System.Collections.Concurrent;
using DoenaSoft.MediaInfoHelper.DataObjects.VideoMetaXml;
using DoenaSoft.MediaInfoHelper.Helpers;
using DoenaSoft.SeriesList.Xml;
using DoenaSoft.ToolBox.Generics;

namespace DoenaSoft.SeriesList;

public static class LanguageHelper
{
    private static readonly object _lock;

    static LanguageHelper()
    {
        _lock = new();
    }

    public static void EnrichLanguages(Xml.RootItem rootItem)
    {
        var seasonsByDrive = (rootItem?.Series ?? [])
            .SelectMany(s => s.Season ?? [])
            .GroupBy(s => GetDrive(s.FullPath));

        Parallel.ForEach(seasonsByDrive, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, GetLanguages);
    }

    private static void GetLanguages(IEnumerable<Season> seasons)
    {
        foreach (var season in seasons)
        {
            lock (_lock)
            {
                Console.WriteLine(season.FullPath);
            }

            season.Languages = GetLanguages(season.FullPath);
        }
    }

    private static string GetDrive(string fullPath)
    {
        var split = fullPath.Split('\\');

        return split[1];
    }

    private static string GetLanguages(string folder)
    {
        var files = Directory.GetFiles(folder, "*.xml", SearchOption.AllDirectories);

        var languagesLists = new BlockingCollection<List<string>>();
        Parallel.ForEach(files, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, file =>
        {
            var subResult = TryAddLanguages(file);

            languagesLists.Add(subResult);
        });

        var languages = languagesLists
            .SelectMany(l => l)
            .StandardizeLanguage()
            .OrderBy(LanguageExtensions.GetLanguageWeight)
            .ToList();

        string text;
        if (languages.Count == 0)
        {
            text = string.Empty;
        }
        else if (languages.Count == 1)
        {
            text = languages[0];
        }
        else
        {
            text = string.Join(", ", languages);
        }

        return text;
    }

    private static List<string> TryAddLanguages(string infoFile)
    {
        try
        {
            using var fs = new FileStream(infoFile, FileMode.Open, FileAccess.Read, FileShare.Read);

            var doc = XmlSerializer<VideoInfoDocument>.Deserialize(fs);

            var languages = doc.VideoInfo?.Audio?.Select(a => a.Language) ?? Enumerable.Empty<string>();

            return languages.ToList();
        }
        catch
        {
            return new List<string>(0);
        }
    }
}