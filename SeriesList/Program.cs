using DoenaSoft.SeriesList;
using DoenaSoft.SeriesList.Configuration;
using DoenaSoft.SeriesList.Xml;

Console.WriteLine(typeof(Program).Assembly.GetName().Version);

try
{
    var threadLock = new object();

    var configuration = new SeriesListConfiguration()
    {
        SeasonFolderPatterns = ["Season ", "Staffel "],
        ArticlesToSkip = ["the", "a", "an", "der", "das"],
        RootPaths = [@"N:\Drive1\TVShows\", @"N:\Drive2\TVShows\", @"N:\Drive3\TVShows\", @"N:\Drive4\TVShows\"],
        MaxDegreeOfParallelism = 4,
        GetPathSegmentForGrouping = (fullPath) =>
        {
            var split = fullPath.Split('\\');

            return split[1];
        },
        ExtractSeriesName = (fullPath) =>
        {
            var parts = fullPath.Split('\\');

            return parts[parts.Length - 2]; // Parent folder is series name
        },
        ExtractSeasonName = (fullPath) =>
        {
            var parts = fullPath.Split('\\');

            return parts[parts.Length - 1]; // Folder name is season name
        },
        CleanPath = (fullPath) =>
        {
            if (fullPath.StartsWith(@"N:\", StringComparison.InvariantCultureIgnoreCase))
            {
                return fullPath.Substring(2).Replace("\\", "/").TrimEnd('/') + "/";
            }

            return fullPath;
        },
        Feedback = (message) =>
        {
            lock (threadLock)
            {
                Console.WriteLine(message);
            }
        },
    };

    var folderGetter = new FolderGetter(configuration);

    var folders = folderGetter.Get();

    var rootItem = XmlCreator.Create(folders);

    var languageHelper = new LanguageHelper(configuration);

    languageHelper.EnrichLanguages(rootItem);

    var cleaner = new Cleaner(configuration);

    cleaner.Clean(rootItem);

    Serializer.Serialize(rootItem, @"N:\Drive1\TVShows\CompleteSeriesList.xml");
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    Console.WriteLine("Press <Enter> to exit.");
    Console.ReadLine();
}