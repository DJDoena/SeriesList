using DoenaSoft.MediaInfoHelper.DataObjects.VideoMetaXml;
using DoenaSoft.MediaInfoHelper.Helpers;
using DoenaSoft.SeriesList.Configuration;
using DoenaSoft.ToolBox.Generics;
using System.Collections.Concurrent;

namespace DoenaSoft.SeriesList;

public class LanguageHelper
{
    private readonly SeriesListConfiguration _configuration;

    public LanguageHelper(SeriesListConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public void EnrichLanguages(Xml.RootItem rootItem)
    {
        var seasonsByDrive = (rootItem?.Series ?? [])
            .SelectMany(s => s.Season ?? [])
            .GroupBy(s => _configuration.GetPathSegmentForGrouping(s.FullPath));

        Parallel.ForEach(seasonsByDrive, new ParallelOptions() { MaxDegreeOfParallelism = _configuration.MaxDegreeOfParallelism }, this.GetLanguages);
    }

    private void GetLanguages(IEnumerable<Xml.Season> seasons)
    {
        foreach (var season in seasons)
        {
            _configuration.Feedback(season.FullPath);

            season.Languages = this.GetLanguages(season.FullPath);
        }
    }

    private string GetLanguages(string folder)
    {
        var files = Directory.GetFiles(folder, "*.xml", SearchOption.AllDirectories);

        var languagesLists = new BlockingCollection<List<string>>();
        Parallel.ForEach(files, new ParallelOptions() { MaxDegreeOfParallelism = _configuration.MaxDegreeOfParallelism }, file =>
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

            return [.. languages];
        }
        catch
        {
            return [];
        }
    }
}