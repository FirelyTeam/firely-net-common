using System.Xml;

namespace Hl7.Fhir.Serialization
{
    internal static class XmlReaderExtensions
    {
        internal static string GenerateLocationMessage(this XmlReader reader)
        {
            return GenerateLocationMessage(reader, out var _, out var _);
        }

        internal static string GenerateLocationMessage(this XmlReader reader, out long lineNumber, out long position)
        {
            (lineNumber, position) = GenerateLineInfo(reader);
            return $"At line {lineNumber}, position {position}.";
        }

        internal static (int lineNumber, int position) GenerateLineInfo(this XmlReader reader)
        {
            IXmlLineInfo xmlInfo = (IXmlLineInfo)reader;
            return (xmlInfo.LineNumber, xmlInfo.LinePosition);
        }

    }
}
