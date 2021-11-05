/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */


#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;

#nullable enable

namespace Hl7.Fhir.Serialization
{
    /// <summary>
    /// An error found during deserialization of Json data. These errors do not present issues
    /// with the Json itself, but issues in the data with regards to the rules for FHIR Json format described
    /// in http://hl7.org/fhir/json.html.
    /// </summary>
    public class JsonFhirException : JsonException
    {
        // TODO: Document each of these errors, based on the text for the error.
        public const string EXPECTED_START_OF_OBJECT_CODE = "JSON101";
        public const string RESOURCETYPE_SHOULD_BE_STRING_CODE = "JSON102";
        public const string NO_RESOURCETYPE_PROPERTY_CODE = "JSON103";
        public const string EXPECTED_PRIMITIVE_NOT_OBJECT_CODE = "JSON104";
        public const string EXPECTED_PRIMITIVE_NOT_ARRAY_CODE = "JSON105";
        public const string INCORRECT_BASE64_DATA_CODE = "JSON106";
        public const string STRING_ISNOTAN_INSTANT_CODE = "JSON107";
        public const string NUMBER_CANNOT_BE_PARSED_CODE = "JSON108";
        public const string EXPECTED_PRIMITIVE_NOT_NULL_CODE = "JSON109";
        public const string UNEXPECTED_JSON_TOKEN_CODE = "JSON110";
        public const string EXPECTED_START_OF_ARRAY_CODE = "JSON111";
        public const string USE_OF_UNDERSCORE_ILLEGAL_CODE = "JSON113";
        public const string CHOICE_ELEMENT_HAS_NO_TYPE_CODE = "JSON114";
        public const string CHOICE_ELEMENT_HAS_UNKOWN_TYPE_CODE = "JSON115";
        public const string UNKNOWN_RESOURCE_TYPE_CODE = "JSON116";
        public const string RESOURCE_TYPE_NOT_A_RESOURCE_CODE = "JSON117";
        public const string UNKNOWN_PROPERTY_FOUND_CODE = "JSON118";
        public const string RESOURCETYPE_UNEXPECTED_CODE = "JSON119";
        public const string OBJECTS_CANNOT_BE_EMPTY_CODE = "JSON120";
        public const string ARRAYS_CANNOT_BE_EMPTY_CODE = "JSON121";
        public const string PRIMITIVE_ARRAYS_INCOMPAT_SIZE_CODE = "JSON122";
        public const string PRIMITIVE_ARRAYS_BOTH_NULL_CODE = "JSON123";
        public const string PRIMITIVE_ARRAYS_LONELY_NULL_CODE = "JSON124";
        public const string PRIMITIVE_ARRAYS_ONLY_NULL_CODE = "JSON125";
        public const string INCOMPATIBLE_SIMPLE_VALUE_CODE = "JSON126";
        public const string CHOICE_ELEMENT_TYPE_NOT_ALLOWED_CODE = "JSON127";

        internal static readonly JsonFhirException EXPECTED_START_OF_OBJECT = new(EXPECTED_START_OF_OBJECT_CODE, "Expected start of object, but found {0}.");
        internal static readonly JsonFhirException RESOURCETYPE_SHOULD_BE_STRING = new(RESOURCETYPE_SHOULD_BE_STRING_CODE, "Property 'resourceType' should be a string, but found {0}.");
        internal static readonly JsonFhirException NO_RESOURCETYPE_PROPERTY = new(NO_RESOURCETYPE_PROPERTY_CODE, "Resource has no 'resourceType' property.");
        internal static readonly JsonFhirException EXPECTED_PRIMITIVE_NOT_OBJECT = new(EXPECTED_PRIMITIVE_NOT_OBJECT_CODE, "Expected a primitive value, not a json object.");
        internal static readonly JsonFhirException EXPECTED_PRIMITIVE_NOT_ARRAY = new(EXPECTED_PRIMITIVE_NOT_ARRAY_CODE, "Expected a primitive value, not the start of an array.");
        internal static readonly JsonFhirException INCORRECT_BASE64_DATA = new(INCORRECT_BASE64_DATA_CODE, "Encountered incorrectly encoded base64 data.");
        internal static readonly JsonFhirException STRING_ISNOTAN_INSTANT = new(STRING_ISNOTAN_INSTANT_CODE, "Literal string '{0}' cannot be parsed as an instant.");
        internal static readonly JsonFhirException NUMBER_CANNOT_BE_PARSED = new(NUMBER_CANNOT_BE_PARSED_CODE, "Json number '{0}' cannot be parsed as a {1}.");
        internal static readonly JsonFhirException EXPECTED_PRIMITIVE_NOT_NULL = new(EXPECTED_PRIMITIVE_NOT_NULL_CODE, "Expected a primitive value, not a json null.");
        internal static readonly JsonFhirException UNEXPECTED_JSON_TOKEN = new(UNEXPECTED_JSON_TOKEN_CODE, "Expecting a {0}, but found a json {1} with value '{2}'.");
        internal static readonly JsonFhirException EXPECTED_START_OF_ARRAY = new(EXPECTED_START_OF_ARRAY_CODE, "Expected start of array since '{0}' is a repeating element.");
        internal static readonly JsonFhirException USE_OF_UNDERSCORE_ILLEGAL = new(USE_OF_UNDERSCORE_ILLEGAL_CODE, "Element '{0}' is not a FHIR primitive, so it should not use an underscore in the '{1}' property.");
        internal static readonly JsonFhirException CHOICE_ELEMENT_HAS_NO_TYPE = new(CHOICE_ELEMENT_HAS_NO_TYPE_CODE, "Choice element '{0}' is not suffixed with a type.");
        internal static readonly JsonFhirException CHOICE_ELEMENT_HAS_UNKOWN_TYPE = new(CHOICE_ELEMENT_HAS_UNKOWN_TYPE_CODE, "Choice element '{0}' is suffixed with an unrecognized type '{1}'.");
        internal static readonly JsonFhirException UNKNOWN_RESOURCE_TYPE = new(UNKNOWN_RESOURCE_TYPE_CODE, "Unknown type '{0}' found in 'resourceType' property.");
        internal static readonly JsonFhirException RESOURCE_TYPE_NOT_A_RESOURCE = new(RESOURCE_TYPE_NOT_A_RESOURCE_CODE, "Data type '{0}' in property 'resourceType' is not a type of resource.");
        internal static readonly JsonFhirException UNKNOWN_PROPERTY_FOUND = new(UNKNOWN_PROPERTY_FOUND_CODE, "Encountered unrecognized property '{0}'.");
        internal static readonly JsonFhirException RESOURCETYPE_UNEXPECTED = new(RESOURCETYPE_UNEXPECTED_CODE, "The 'resourceType' property should only be used in resources.");
        internal static readonly JsonFhirException OBJECTS_CANNOT_BE_EMPTY = new(OBJECTS_CANNOT_BE_EMPTY_CODE, "An object needs to have at least one property.");
        internal static readonly JsonFhirException ARRAYS_CANNOT_BE_EMPTY = new(ARRAYS_CANNOT_BE_EMPTY_CODE, "An array needs to have at least one element.");
        internal static readonly JsonFhirException PRIMITIVE_ARRAYS_INCOMPAT_SIZE = new(PRIMITIVE_ARRAYS_INCOMPAT_SIZE_CODE, "Primitive arrays split in two properties should have the same size.");
        internal static readonly JsonFhirException PRIMITIVE_ARRAYS_BOTH_NULL = new(PRIMITIVE_ARRAYS_BOTH_NULL_CODE, "Primitive arrays split in two properties should not both have a null at the same position.");
        internal static readonly JsonFhirException PRIMITIVE_ARRAYS_LONELY_NULL = new(PRIMITIVE_ARRAYS_LONELY_NULL_CODE, "Property '{0}' is a single primitive array and should not contain a null.");
        internal static readonly JsonFhirException PRIMITIVE_ARRAYS_ONLY_NULL = new(PRIMITIVE_ARRAYS_ONLY_NULL_CODE, "If present, property '{0}' should not only contain nulls.");
        internal static readonly JsonFhirException INCOMPATIBLE_SIMPLE_VALUE = new(INCOMPATIBLE_SIMPLE_VALUE_CODE, "Found a json primitive value that does not match the expected type of the primitive property. Details: {0}");
        internal static readonly JsonFhirException CHOICE_ELEMENT_TYPE_NOT_ALLOWED = new(CHOICE_ELEMENT_TYPE_NOT_ALLOWED_CODE, "Choice element '{0}' is suffixed with type '{1}', which is not allowed here.");

        /// <summary>
        /// The set of errors for which the deserializer is able to retain the data present in the json file, even when the data
        /// does not fully comply to the FHIR json serialization rules.
        /// This means that, even though there are errors, developers may chose to continue data processing, keeping in mind that
        /// the data in the produced POCO instance may not be valid.
        /// 
        /// NOTE: It is possible that the errors encountered are caused by deeper issues with the serializer of the json data.
        /// The data returned by the deserializer after finding these recoverable error may therefore still have lost informational
        /// content. Proceed with care.
        /// </summary>
        public static readonly string[] RECOVERABLE_ERROR_CODES = new[]
        { 
            // The serialization contained a json null where it is not allowed, but a null does not contain data anyway.
            EXPECTED_PRIMITIVE_NOT_NULL_CODE,

            // These errors signal parsing errors, but the original raw data is retained in the POCO so no data is lost.
            INCORRECT_BASE64_DATA_CODE, STRING_ISNOTAN_INSTANT_CODE, NUMBER_CANNOT_BE_PARSED_CODE, UNEXPECTED_JSON_TOKEN_CODE,

            // The parser will turn a non-array value into an array with a single element, so no data is lost.
            EXPECTED_START_OF_ARRAY_CODE,

            // We will just ignore the underscore and keep on parsing
            USE_OF_UNDERSCORE_ILLEGAL_CODE,

            // The serialization contained a superfluous 'resourceType' property, but we have read all data anyway.
            RESOURCETYPE_UNEXPECTED_CODE,

            // Empty objects and arrays can be ignored without discarding data
            OBJECTS_CANNOT_BE_EMPTY_CODE, ARRAYS_CANNOT_BE_EMPTY_CODE,

            // Shortest array will be filled out with nulls
            PRIMITIVE_ARRAYS_INCOMPAT_SIZE_CODE,
            
            // This leaves the incorrect nulls in place, no change in data.
            PRIMITIVE_ARRAYS_BOTH_NULL_CODE, PRIMITIVE_ARRAYS_LONELY_NULL_CODE, PRIMITIVE_ARRAYS_ONLY_NULL_CODE
        };

        /// <summary>
        /// The unique and permanent code for this error.
        /// </summary>
        /// <remarks>Developers can assume that these codes will not change in future versions.</remarks>
        public string ErrorCode { get; private set; }

        public JsonFhirException(string code, string message) :
            this(code, message, lineNumber: null, bytePositionInLine: null, innerException: null)
        {
        }

        public JsonFhirException(string code, string message, Exception? innerException) :
            this(code, message, lineNumber: null, bytePositionInLine: null, innerException)
        {

        }

        public JsonFhirException(string code, string message, long? lineNumber, long? bytePositionInLine) :
            this(code, message, lineNumber, bytePositionInLine, innerException: null)
        {
        }

        public JsonFhirException(string code, string message, long? lineNumber, long? bytePositionInLine, Exception? innerException) : base(message, path: null, lineNumber, bytePositionInLine, innerException)
        {
            ErrorCode = code;
        }

        /// <summary>
        /// Represents an error for which the deserializer is able to retain the data present in the json file, even when the data
        /// does not fully comply to the FHIR json serialization rules. See <see cref="RECOVERABLE_ERROR_CODES"/>.
        /// </summary>
        public bool Recoverable => RECOVERABLE_ERROR_CODES.Contains(ErrorCode);

        internal JsonFhirException With(ref Utf8JsonReader reader, params object?[] parameters) =>
            With(ref reader, inner: null, parameters);

        internal JsonFhirException With(ref Utf8JsonReader reader, JsonFhirException? inner, params object?[] parameters)
        {
            var formattedMessage = string.Format(CultureInfo.InvariantCulture, Message, parameters);

            var location = reader.GenerateLocationMessage(out var lineNumber, out var position);
            var message = formattedMessage + location;            

            return new JsonFhirException(ErrorCode, message, lineNumber, position, inner);
        }
    }
}

#nullable restore
#endif