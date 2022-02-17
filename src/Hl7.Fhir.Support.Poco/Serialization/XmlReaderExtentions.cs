using System.Xml;

namespace Hl7.Fhir.Serialization
{
    internal static class XmlReaderExtentions
    {
        internal static string GenerateLocationMessage(this XmlReader reader)
        {
            return GenerateLocationMessage(reader, out var _, out var _);
        }

        internal static string GenerateLocationMessage(this XmlReader reader, out long lineNumber, out long position)
        {
            IXmlLineInfo xmlInfo = (IXmlLineInfo)reader;
            lineNumber = xmlInfo.LineNumber;
            position = xmlInfo.LinePosition;
            return $"At line {lineNumber}, position {position}.";
        }
    }
}
