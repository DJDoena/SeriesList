using System.Diagnostics;

namespace DoenaSoft.SeriesList;

[DebuggerDisplay("{Season}")]
internal sealed class SeriesValue
{
    public string Season { get; }

    public string FullPath { get; }

    public SeriesValue(string season, string fullPath)
    {
        this.Season = season;
        this.FullPath = fullPath;
    }
}