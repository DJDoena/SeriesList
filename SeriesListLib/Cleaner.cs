using DoenaSoft.SeriesList.Configuration;
using DoenaSoft.SeriesList.Xml;

namespace DoenaSoft.SeriesList;

public class Cleaner
{
    private readonly SeriesListConfiguration _configuration;

    public Cleaner(SeriesListConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public void Clean(RootItem rootItem)
    {
        if (!Clean(ref rootItem.Series))
        {
            rootItem.Series = null;
        }
    }

    private bool Clean(ref List<Series> seriesList)
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

    private bool Clean(ref List<Season> seasonList)
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
                if (!string.IsNullOrEmpty(season.FullPath))
                {
                    season.FullPath = _configuration.CleanPath(season.FullPath);
                }
            }

            return true;
        }
    }
}