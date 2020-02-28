/* 
 * Copyright (c) 2018, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using Hl7.Fhir.Utility;
using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Hl7.Fhir.Serialization
{
    internal static class XObjectFhirXmlExtensions
    {
        public static bool IsResourceName(this XName elementName) =>
            Char.IsUpper(elementName.LocalName, 0) && elementName.Namespace == XmlNs.XFHIR;

        public static bool TryGetContainedResource(this XElement xe, out XElement contained)
        {
            contained = null;

            if (xe.HasElements)
            {
                var candidate = xe.Elements().First();

                if (candidate.Name.IsResourceName())
                {
                    contained = candidate;
                    return true;
                }
            }

            // Not on a resource, no name to be found
            return false;
        }

        public static XObject NextElementOrAttribute(this XObject current)
        {
            var scan = current.NextSibling();
            return scanToNextRelevantNode(scan);
        }

        public static XObject FirstChildElementOrAttribute(this XObject current)
        {
            var scan = current.FirstChild();
            return scanToNextRelevantNode(scan);
        }


        private static XObject scanToNextRelevantNode(this XObject scan)
        {
            while (scan != null)
            {
                if (IsRelevantNode(scan))
                    break;
                scan = scan.NextSibling();
            }

            return scan;
        }

        public static bool IsRelevantNode(this XObject scan) =>
            scan.NodeType == XmlNodeType.Element ||
                   (scan is XAttribute attr && isRelevantAttribute(attr));

        private static bool isRelevantAttribute(XAttribute a) =>
            !a.IsNamespaceDeclaration && a.Name != XmlNs.XSCHEMALOCATION;

        public static bool HasRelevantAttributes(this XElement scan) =>
            scan.Attributes().Any(a => isRelevantAttribute(a));

        public static string GetValue(this XObject current)
        {
            // If this node has XHTML content, serialize the node into a string as the value
            if (current.AtXhtmlDiv())
                return ((XElement)current).ToString(SaveOptions.DisableFormatting);

            // If this is an xml attribute, return its value
            else if (current is XAttribute xattr)
                return xattr.Value;

            else if (current is XElement xelem)
            {
                var valueVal = xelem.Attribute("value")?.Value.Trim();

                // If there is a `value` attribute, return its content (regardless whether there are 
                // nested nodes).
                if (valueVal != null)
                    return valueVal;
                else
                {
                    // If this is an element with text content (or mixed mode), return the text from the first text node
                    if (current.FirstChild() is XText txt)
                        return txt.Value;
                }                
            }

            // In all other cases, there is no value
            return null;
        }
    }
}
