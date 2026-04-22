using DoenaSoft.SeriesList.Comparer;

namespace DoenaSoft.SeriesList.DataObjects;

public sealed class SeriesKey : Tuple<string>, IComparable<SeriesKey>
{
    private static readonly IComparer<string> _comparer;

    private readonly IEnumerable<string> _articlesToSkip;

    public string SeriesName
        => this.Item1;

    static SeriesKey()
    {
        _comparer = new HumanComparer();
    }

    internal SeriesKey(string seriesName
        , IEnumerable<string> articlesToSkip)
        : base(seriesName)
    {
        _articlesToSkip = articlesToSkip ?? [];
    }

    public int CompareTo(SeriesKey other)
    {
        if (other is null)
        {
            return ComparisonResults.LeftGreaterThanRight;
        }

        var left = this.SeriesName;

        var right = other.SeriesName;

        this.ProcessTitle(ref left);

        this.ProcessTitle(ref right);

        var result = _comparer.Compare(left, right);

        return result;
    }

    private void ProcessTitle(ref string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        var split = title.Split(' ');

        // Split always returns at least one element
        // If there's only one word, there's no article to skip
        if (split.Length == 1)
        {
            return;
        }

        var skip = 0;

        foreach (var article in _articlesToSkip)
        {
            if (split[0].Equals(article, StringComparison.CurrentCultureIgnoreCase))
            {
                skip = 1;

                break;
            }
        }

        title = string.Join(" ", split.Skip(skip));
    }
}