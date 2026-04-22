using System.Diagnostics;

namespace DoenaSoft.SeriesList.DataObjects;

[DebuggerDisplay("{Season}")]
public readonly struct SeriesValue
{
    public string Season { get; }

    public string FullPath { get; }

    internal SeriesValue(string season, string fullPath)
    {
        this.Season = season;
        this.FullPath = fullPath;
    }
}