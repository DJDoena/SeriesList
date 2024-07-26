using DoenaSoft.SeriesList.Comparer;

namespace DoenaSoft.SeriesList;

internal sealed class SeriesKey : Tuple<string>, IComparable<SeriesKey>
{
    private static readonly IComparer<string> _comparer;

    public string SeriesName
        => this.Item1;

    static SeriesKey()
    {
        _comparer = new HumanComparer();
    }

    public SeriesKey(string seriesName) : base(seriesName)
    {
    }

    public int CompareTo(SeriesKey other)
    {
        if (other is null)
        {
            return ComparisonResults.LeftGreaterThanRight;
        }

        var left = this.SeriesName;

        var right = other.SeriesName;

        ProcessTitle(ref left);

        ProcessTitle(ref right);

        var result = _comparer.Compare(left, right);

        return result;
    }

    private static void ProcessTitle(ref string title)
    {
        var split = title.Split(' ');

        var skip = 0;

        if (split[0].Equals("the", StringComparison.CurrentCultureIgnoreCase))
        {
            skip = 1;
        }
        else if (split[0].Equals("a", StringComparison.CurrentCultureIgnoreCase))
        {
            skip = 1;
        }
        else if (split[0].Equals("der", StringComparison.CurrentCultureIgnoreCase))
        {
            skip = 1;
        }

        title = string.Join(" ", split.Skip(skip));
    }
}