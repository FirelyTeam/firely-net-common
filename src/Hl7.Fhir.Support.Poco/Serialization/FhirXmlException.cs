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
        public const string UNKNOWN_PROPERTY_FOUND_CODE = "XML104";
        public const string CHOICE_ELEMENT_HAS_NO_TYPE_CODE = "XML105";
        public const string CHOICE_ELEMENT_HAS_UNKOWN_TYPE_CODE = "XML106";
        public const string STRING_ISNOTAN_INSTANT_CODE = "XML107";
        public const string INCORRECT_BASE64_DATA_CODE = "XML108";
        public const string STRING_ISNOTAN_INT_CODE = "XML109";
        public const string STRING_ISNOTA_LONG_CODE = "XML110";
        public const string STRING_ISNOTA_DECIMAL_CODE = "XML111";
        public const string STRING_ISNOTA_BOOLEAN_CODE = "XML112";

        internal static readonly FhirXmlException INCORRECT_ROOT_NAMESPACE = new(INCORRECT_ROOT_NAMESPACE_CODE, "Root has missing or incorrect namespace. Namespace should be \"http://hl7.org/fhir\"");
        internal static readonly FhirXmlException UNKNOWN_RESOURCE_TYPE = new(UNKNOWN_RESOURCE_TYPE_CODE, "Unknown type '{0}' found in root property.");
        internal static readonly FhirXmlException RESOURCE_TYPE_NOT_A_RESOURCE = new(RESOURCE_TYPE_NOT_A_RESOURCE_CODE, "Data type '{0}' in property 'resourceType' is not a type of resource.");
        internal static readonly FhirXmlException UNKNOWN_PROPERTY_FOUND = new(UNKNOWN_PROPERTY_FOUND_CODE, "Encountered unrecognized property '{0}'.");
        internal static readonly FhirXmlException CHOICE_ELEMENT_HAS_NO_TYPE = new(CHOICE_ELEMENT_HAS_NO_TYPE_CODE, "Choice element '{0}' is not suffixed with a type.");
        internal static readonly FhirXmlException CHOICE_ELEMENT_HAS_UNKOWN_TYPE = new(CHOICE_ELEMENT_HAS_UNKOWN_TYPE_CODE, "Choice element '{0}' is suffixed with an unrecognized type '{1}'.");
        internal static readonly FhirXmlException STRING_ISNOTAN_INSTANT = new(STRING_ISNOTAN_INSTANT_CODE, "Literal string '{0}' cannot be parsed as an instant.");
        internal static readonly FhirXmlException INCORRECT_BASE64_DATA = new(INCORRECT_BASE64_DATA_CODE, "Encountered incorrectly encoded base64 data.");
        internal static readonly FhirXmlException STRING_ISNOTAN_INT = new(STRING_ISNOTAN_INT_CODE, "Literal string '{0}' cannot be parsed as an integer.");
        internal static readonly FhirXmlException STRING_ISNOTA_LONG = new(STRING_ISNOTA_LONG_CODE, "Literal string '{0}' cannot be parsed as a long integer.");
        internal static readonly FhirXmlException STRING_ISNOTA_DECIMAL = new(STRING_ISNOTA_DECIMAL_CODE, "Literal string '{0}' cannot be parsed as a decimal.");
        internal static readonly FhirXmlException STRING_ISNOTA_BOOLEAN = new(STRING_ISNOTA_BOOLEAN_CODE, "Literal string '{0}' cannot be parsed as a boolean.");

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
