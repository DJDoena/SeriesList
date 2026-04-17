using DoenaSoft.SeriesList.Xml;
using DoenaSoft.ToolBox.Generics;

namespace DoenaSoft.SeriesList;

internal static class Serializer
{
    internal static void Serialize(RootItem rootItem, string fileName)
    {
        (new XsltSerializer<RootItem>(new RootItemXsltSerializerDataProvider())).Serialize(fileName, rootItem);

        File.SetAttributes(fileName, FileAttributes.Archive);
    }
}