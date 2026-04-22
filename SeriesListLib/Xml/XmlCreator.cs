using DoenaSoft.SeriesList.Comparer;
using DoenaSoft.SeriesList.DataObjects;

namespace DoenaSoft.SeriesList.Xml;

public static class XmlCreator
{
    private static readonly IComparer<string> _comparer;

    static XmlCreator()
    {
        _comparer = new HumanComparer();
    }

    public static RootItem Create(ISeriesDictionary source)
    {
        var sortedSeries = source.ToList();

        sortedSeries.Sort((l, r) => l.Key.CompareTo(r.Key));

        var rootItem = new RootItem()
        {
            Series = [.. sortedSeries.Select(ToSeriesXml)],
        };

        return rootItem;
    }

    private static Series ToSeriesXml(KeyValuePair<SeriesKey, SeriesValues> source)
    {
        var sortedSeasons = source.Value;

        sortedSeasons.Sort(CompareSeasons);

        var target = new Series()
        {
            Name = source.Key.SeriesName,
            Season = [.. sortedSeasons.Select(ToSeasonXml)],
        };

        return target;
    }

    private static int CompareSeasons(SeriesValue left, SeriesValue right)
    {
        var compare = _comparer.Compare(left.Season, right.Season);

        if (compare == ComparisonResults.LeftEqualsRight)
        {
            compare = _comparer.Compare(left.FullPath, right.FullPath);
        }

        return compare;
    }

    private static Season ToSeasonXml(SeriesValue source)
    {
        var target = new Season()
        {
            Name = source.Season,
            FullPath = source.FullPath,
        };

        return target;
    }
}