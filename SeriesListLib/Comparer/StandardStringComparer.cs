namespace DoenaSoft.SeriesList.Comparer;

internal static class StandardStringComparer
{
    public static int Compare(string left, string right)
        => string.Compare(left, right);
}