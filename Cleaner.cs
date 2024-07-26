using DoenaSoft.SeriesList.Xml;

namespace DoenaSoft.SeriesList;

internal static class Cleaner
{
    internal static void Clean(RootItem rootItem)
    {
        if (!Clean(ref rootItem.Series))
        {
            rootItem.Series = null;
        }
    }

    private static bool Clean(ref List<Series> seriesList)
    {
        if (seriesList != null && seriesList.Count == 0)
        {
            seriesList = null;

            return false;
        }
        else
        {
            foreach (var series in seriesList)
            {
                if (!Clean(ref series.Season))
                {
                    series.Season = null;
                }
            }

            return true;
        }
    }

    private static bool Clean(ref List<Season> seasonList)
    {
        if (seasonList != null && seasonList.Count == 0)
        {
            seasonList = null;

            return false;
        }
        else
        {
            foreach (var season in seasonList)
            {
                var path = season.FullPath;

                if (!string.IsNullOrEmpty(path) && path.StartsWith(@"N:\", StringComparison.InvariantCultureIgnoreCase))
                {
                    var cleanedPath = path.Substring(2).Replace("\\", "/").TrimEnd('/') + "/";

                    season.FullPath = cleanedPath;
                }
            }

            return true;
        }
    }
}
