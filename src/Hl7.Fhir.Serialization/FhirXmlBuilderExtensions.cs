/* 
 * Copyright (c) 2018, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */


using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Utility;
using System.Xml;
using System.Xml.Linq;

namespace Hl7.Fhir.Serialization
{
    public static class FhirXmlBuilderExtensions
    {
        private static void writeTo(this XDocument doc, XmlWriter destination)
        {
            if (doc.Root != null)
                doc.WriteTo(destination);

            destination.Flush();
        }

        public static void WriteTo(this ISourceNode source, XmlWriter destination, FhirXmlSerializationSettings settings = null) =>
            new FhirXmlBuilder(settings).Build(source).writeTo(destination);

        public static void WriteTo(this ITypedElement source, XmlWriter destination, FhirXmlSerializationSettings settings = null) =>
            new FhirXmlBuilder(settings).Build(source).writeTo(destination);

        public static XDocument ToXDocument(this ISourceNode source, FhirXmlSerializationSettings settings = null) =>
            new FhirXmlBuilder(settings).Build(source);

        public static XDocument ToXDocument(this ITypedElement source, FhirXmlSerializationSettings settings = null) =>
            new FhirXmlBuilder(settings).Build(source);

        public static string ToXml(this ISourceNode source, FhirXmlSerializationSettings settings = null)
        => SerializationUtil.WriteXmlToString(writer => source.WriteTo(writer, settings), settings?.Pretty ?? false, settings?.AppendNewLine ?? false);

        public static string ToXml(this ITypedElement source, FhirXmlSerializationSettings settings = null)
                => SerializationUtil.WriteXmlToString(writer => source.WriteTo(writer, settings), settings?.Pretty ?? false, settings?.AppendNewLine ?? false);

        public static byte[] ToXmlBytes(this ITypedElement source, FhirXmlSerializationSettings settings = null)
                => SerializationUtil.WriteXmlToBytes(writer => source.WriteTo(writer, settings));
    }
}
