using DoenaSoft.SeriesList;

Console.WriteLine(typeof(Program).Assembly.GetName().Version);

try
{
    var folders = FolderGetter.Get();

    var rootItem = XmlCreator.Create(folders);

    LanguageHelper.EnrichLanguages(rootItem);

    Cleaner.Clean(rootItem);

    Serializer.Serialize(rootItem, @"N:\Drive1\TVShows\CompleteSeriesList.xml");
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    Console.WriteLine("Press <Enter> to exit.");
    Console.ReadLine();
}
