using System.ComponentModel;

namespace DoenaSoft.SeriesList.Comparer;

internal sealed class HumanComparer : IComparer<string>
{
    private readonly ListSortDirection _direction;

    public HumanComparer(ListSortDirection direction)
    {
        _direction = direction;
    }

    public HumanComparer()
    {
        _direction = ListSortDirection.Ascending;
    }

    public int Compare(string left, string right)
    {
        var compare = StringChunksComparer.Compare(left, right);

        var result = _direction == ListSortDirection.Descending
            ? -compare
            : compare;

        return result;
    }
}