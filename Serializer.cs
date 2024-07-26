using System.Text;
using System.Xml;
using System.Xml.Serialization;
using DoenaSoft.SeriesList.Xml;

namespace DoenaSoft.SeriesList;

internal static class Serializer
{
    internal static void Serialize(RootItem rootItem, string fileName)
    {
        var xml = GetXmlString(rootItem);

        xml = "\t" + xml.TrimStart().Replace(Environment.NewLine, Environment.NewLine + "\t");

        using (var sw = new StreamWriter(fileName, false, Encoding.UTF8))
        {
            sw.WriteLine(GetPrefix());
            sw.WriteLine(xml);
            sw.Write(GetSuffix());
        }

        File.SetAttributes(fileName, FileAttributes.Archive);
    }

    private static string GetXmlString(RootItem rootItem)
    {
        using var ms = new MemoryStream();

        var encoding = Encoding.UTF8;

        var settings = new XmlWriterSettings()
        {
            Encoding = encoding,
            Indent = true,
            OmitXmlDeclaration = true,
            IndentChars = "\t",
        };

        using var writer = XmlWriter.Create(ms, settings);

        var serializer = new XmlSerializer(typeof(RootItem));

        serializer.Serialize(writer, rootItem, new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));

        var xml = encoding.GetString(ms.ToArray());

        return xml;
    }

    private static string GetPrefix()
        => "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<?xml-stylesheet type=\"text/xml\" href=\"#stylesheet\"?>\r\n<!DOCTYPE doc [\r\n<!ATTLIST xsl:stylesheet\r\n    id    ID    #REQUIRED>\r\n]>\r\n<doc>";

    private static string GetSuffix()
        => "\t<xsl:stylesheet id=\"stylesheet\" version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\t\t\r\n\t\t<xsl:template name=\"PrintSeason\">\r\n\t\t\t<xsl:param name=\"item\" />\r\n\t\t\t<xsl:variable name=\"space\">&#160;</xsl:variable>\r\n\t\t\t\t<li>\r\n\t\t\t\t\t<xsl:value-of select=\"@Name\"/>\r\n\t\t\t\t\t\t<xsl:choose>\r\n\t\t\t\t\t\t\t<xsl:when test=\"@Languages != ''\">\r\n\t\t\t\t\t\t\t\t<xsl:value-of select=\"$space\" /><xsl:value-of select=\"$space\" /><xsl:value-of select=\"$space\" />(Languages: <xsl:value-of select=\"@Languages\"/>)\r\n\t\t\t\t\t\t\t</xsl:when>\r\n\t\t\t\t\t\t</xsl:choose>\r\n\t\t\t\t\t\t<xsl:choose>\r\n\t\t\t\t\t\t\t<xsl:when test=\"$item/FullPath != ''\">\r\n\t\t\t\t\t\t\t\t<xsl:value-of select=\"$space\" /><xsl:value-of select=\"$space\" /><xsl:value-of select=\"$space\" /><xsl:value-of select=\"$space\" /><xsl:value-of select=\"$space\" /><span style=\"font-family: monospace;\"><xsl:value-of select=\"$item/FullPath\" /></span>\r\n\t\t\t\t\t\t\t</xsl:when>\r\n\t\t\t\t\t</xsl:choose>\r\n\t\t\t\t</li>\r\n\t\t</xsl:template>\r\n\t\t\r\n\t\t<xsl:template name=\"PrintAllSeasons\">\r\n\t\t\t<xsl:param name=\"items\" />\r\n\t\t\t<xsl:if test=\"count($items) > 0\">\r\n\t\t\t\t<ul>\r\n\t\t\t\t\t<xsl:for-each select=\"$items\">\r\n\t\t\t\t\t\t<xsl:call-template name=\"PrintSeason\">\r\n\t\t\t\t\t\t\t<xsl:with-param name=\"item\" select=\"current()\" />\r\n\t\t\t\t\t\t</xsl:call-template>\t\r\n\t\t\t\t\t</xsl:for-each>\r\n\t\t\t\t</ul>\r\n\t\t\t</xsl:if>\r\n\t\t</xsl:template>\r\n\t\r\n\t\t<xsl:template name=\"PrintSeries\">\r\n\t\t\t<xsl:param name=\"item\" />\r\n\t\t\t\t<li>\r\n\t\t\t\t\t<span style=\"color: navy;\"><xsl:value-of select=\"@Name\"/></span>\r\n\t\t\t\t\t<xsl:call-template name=\"PrintAllSeasons\">\r\n\t\t\t\t\t\t<xsl:with-param name=\"items\" select=\"$item/Season\" />\r\n\t\t\t\t\t</xsl:call-template>\r\n\t\t\t\t</li>\r\n\t\t</xsl:template>\r\n\t\t\t\t\r\n\t\t<xsl:template name=\"PrintAllSeries\">\r\n\t\t\t<xsl:param name=\"items\" />\r\n\t\t\t<xsl:if test=\"count($items) > 0\">\r\n\t\t\t\t<ul>\r\n\t\t\t\t\t<xsl:for-each select=\"$items\">\r\n\t\t\t\t\t\t<xsl:call-template name=\"PrintSeries\">\r\n\t\t\t\t\t\t\t<xsl:with-param name=\"item\" select=\"current()\" />\r\n\t\t\t\t\t\t</xsl:call-template>\t\r\n\t\t\t\t\t</xsl:for-each>\r\n\t\t\t\t</ul>\r\n\t\t\t</xsl:if>\r\n\t\t</xsl:template>\r\n\r\n\t\t<xsl:template match=\"/\">\r\n\t\t\t<html>\r\n\t\t\t\t<body>\r\n\t\t\t\t\t<xsl:call-template name=\"PrintAllSeries\">\r\n\t\t\t\t\t\t<xsl:with-param name=\"items\" select=\"//doc/RootItem/Series\" />\r\n\t\t\t\t\t</xsl:call-template>\r\n\t\t\t\t</body>\r\n\t\t\t</html>\r\n\t\t</xsl:template>\r\n\t</xsl:stylesheet>\r\n</doc>";
}