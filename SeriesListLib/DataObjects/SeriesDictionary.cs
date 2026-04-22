namespace DoenaSoft.SeriesList.DataObjects;

public sealed class SeriesDictionary : Dictionary<SeriesKey, SeriesValues>, ISeriesDictionary
{
    internal SeriesDictionary(int count)
        : base(count)
    {
    }
}