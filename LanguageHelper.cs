using DoenaSoft.MediaInfoHelper.DataObjects.VideoMetaXml;
using DoenaSoft.MediaInfoHelper.Helpers;
using DoenaSoft.ToolBox.Generics;

namespace DoenaSoft.SeriesList;

internal static class LanguageHelper
{
    internal static void EnrichLanguages(Xml.RootItem rootItem)
    {
        var seasons = (rootItem?.Series ?? Enumerable.Empty<Xml.Series>())
            .SelectMany(s => s.Season ?? Enumerable.Empty<Xml.Season>());

        foreach (var season in seasons)
        {
            season.Languages = GetLanguages(season.FullPath);
        }
    }

    private static string GetLanguages(string folder)
    {
        var files = Directory.GetFiles(folder, "*.xml", SearchOption.AllDirectories);

        var tasks = new List<Task<List<string>>>(files.Length);

        foreach (var file in files)
        {
            tasks.Add(Task.Run(() => TryAddLanguages(file)));
        }

        Task.WaitAll(tasks.ToArray());

        var languages = tasks
            .SelectMany(t => t.Result)
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