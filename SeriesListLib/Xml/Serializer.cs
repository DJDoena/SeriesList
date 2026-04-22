using DoenaSoft.ToolBox.Generics;

namespace DoenaSoft.SeriesList.Xml;

public static class Serializer
{
    public static void Serialize(RootItem rootItem, string fileName)
    {
        (new XsltSerializer<RootItem>(new RootItemXsltSerializerDataProvider())).Serialize(fileName, rootItem);

        File.SetAttributes(fileName, FileAttributes.Archive);
    }
}