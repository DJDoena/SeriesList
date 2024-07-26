using DoenaSoft.SeriesList.Comparer;
using DoenaSoft.SeriesList.Xml;

namespace DoenaSoft.SeriesList;

internal static class XmlCreator
{
    private static readonly IComparer<string> _comparer;

    static XmlCreator()
    {
        _comparer = new HumanComparer();
    }

    internal static RootItem Create(Dictionary<SeriesKey, List<SeriesValue>> source)
    {
        var sortedSeries = source.ToList();

        sortedSeries.Sort((l, r) => l.Key.CompareTo(r.Key));

        var rootItem = new RootItem()
        {
            Series = sortedSeries.Select(ToSeriesXml).ToList(),
        };

        return rootItem;
    }

    private static Series ToSeriesXml(KeyValuePair<SeriesKey, List<SeriesValue>> source)
    {
        var sortedSeasons = source.Value;

        sortedSeasons.Sort(CompareSeasons);

        var target = new Series()
        {
            Name = source.Key.SeriesName,
            Season = sortedSeasons.Select(ToSeasonXml).ToList(),
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