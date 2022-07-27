#nullable enable

using Hl7.Fhir.Utility;
using System;
using System.Globalization;
using System.Xml;

namespace Hl7.Fhir.Serialization
{
    public class FhirXmlException : CodedException
    {
        public const string INCORRECT_ROOT_NAMESPACE_CODE = "XML101";
        public const string UNKNOWN_RESOURCE_TYPE_CODE = "XML102";
        public const string RESOURCE_TYPE_NOT_A_RESOURCE_CODE = "XML103";
        public const string UNKNOWN_ELEMENT_CODE = "XML104";
        public const string CHOICE_ELEMENT_HAS_NO_TYPE_CODE = "XML105";
        public const string CHOICE_ELEMENT_HAS_UNKNOWN_TYPE_CODE = "XML106";
        public const string INCORRECT_XHTML_NAMESPACE_CODE = "XML107";
        public const string UNKNOWN_ATTRIBUTE_CODE = "XML108";
        public const string UNEXPECTED_ELEMENT_CODE = "XML109";
        public const string UNALLOWED_ElEMENT_IN_RESOURCE_CONTAINER_CODE = "XML110";
        public const string NO_ATTRIBUTES_ALLOWED_ON_RESOURCE_CONTAINER_CODE = "XML111";
        public const string INCORRECT_ELEMENT_NAMESPACE_CODE = "XML112";

        public const string INCORRECT_BASE64_DATA_CODE = "XML202";
        public const string VALUE_IS_NOT_OF_EXPECTED_TYPE_CODE = "XML203";


        internal static readonly FhirXmlException INCORRECT_ROOT_NAMESPACE = new(INCORRECT_ROOT_NAMESPACE_CODE, "Element has incorrect namespace {0}. Namespace should be \"http://hl7.org/fhir\".");
        internal static readonly FhirXmlException INCORRECT_ELEMENT_NAMESPACE = new(INCORRECT_ELEMENT_NAMESPACE_CODE, "Root has missing or incorrect namespace. Namespace should be \"http://hl7.org/fhir\".");
        internal static readonly FhirXmlException UNKNOWN_RESOURCE_TYPE = new(UNKNOWN_RESOURCE_TYPE_CODE, "Unknown type '{0}' found in root property.");
        internal static readonly FhirXmlException RESOURCE_TYPE_NOT_A_RESOURCE = new(RESOURCE_TYPE_NOT_A_RESOURCE_CODE, "Data type '{0}' in property 'resourceType' is not a type of resource.");
        internal static readonly FhirXmlException UNKNOWN_ELEMENT = new(UNKNOWN_ELEMENT_CODE, "Encountered unrecognized element '{0}'.");
        internal static readonly FhirXmlException CHOICE_ELEMENT_HAS_NO_TYPE = new(CHOICE_ELEMENT_HAS_NO_TYPE_CODE, "Choice element '{0}' is not suffixed with a type.");
        internal static readonly FhirXmlException CHOICE_ELEMENT_HAS_UNKOWN_TYPE = new(CHOICE_ELEMENT_HAS_UNKNOWN_TYPE_CODE, "Choice element '{0}' is suffixed with an unrecognized type '{1}'.");
        internal static readonly FhirXmlException INCORRECT_BASE64_DATA = new(INCORRECT_BASE64_DATA_CODE, "Encountered incorrectly encoded base64 data.");
        internal static readonly FhirXmlException VALUE_IS_NOT_OF_EXPECTED_TYPE = new(VALUE_IS_NOT_OF_EXPECTED_TYPE_CODE, "Literal string '{0}' cannot be parsed as a '{1}'.");
        internal static readonly FhirXmlException INCORRECT_XHTML_NAMESPACE = new(INCORRECT_XHTML_NAMESPACE_CODE, "Narrative has missing or incorrect namespace. Namespace should be \"http://www.w3.org/1999/xhtml\"");
        internal static readonly FhirXmlException UNKNOWN_ATTRIBUTE = new(UNKNOWN_ATTRIBUTE_CODE, "Encountered unrecognized attribute '{0}'.");
        internal static readonly FhirXmlException UNEXPECTED_ELEMENT = new(UNEXPECTED_ELEMENT_CODE, "Encountered unexpected element '{0}', please check the order of the xml.");
        internal static readonly FhirXmlException UNALLOWED_ElEMENT_IN_RESOURCE_CONTAINER = new(UNALLOWED_ElEMENT_IN_RESOURCE_CONTAINER_CODE, "Encountered unallowed content '{0}' in the resource container. Only a single resource is allowed.");
        internal static readonly FhirXmlException NO_ATTRIBUTES_ALLOWED_ON_RESOURCE_CONTAINER = new(NO_ATTRIBUTES_ALLOWED_ON_RESOURCE_CONTAINER_CODE, "Encountered xml attributes on resource container {0}. No attributes are allowed.");

        public FhirXmlException(string errorCode, string message) : base(errorCode, message)
        {
        }

        public FhirXmlException(string code, string message, Exception? innerException) : base(code, message, innerException)
        {
        }

        internal FhirXmlException With(XmlReader reader, params object?[] parameters) =>
          With(reader, inner: null, parameters);

        internal FhirXmlException With(XmlReader reader, FhirXmlException? inner, params object?[] parameters)
        {
            var formattedMessage = string.Format(CultureInfo.InvariantCulture, Message, parameters);
            var location = reader.GenerateLocationMessage();
            var message = $"{formattedMessage} {location}";

            return new FhirXmlException(ErrorCode, message, inner);
        }
    }
}

#nullable restore
