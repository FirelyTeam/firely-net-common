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
using System.Text.Json;

#nullable enable

namespace Hl7.Fhir.Serialization
{
    public class JsonFhirException : JsonException
    {
        internal static readonly JsonFhirException EXPECTED_START_OF_OBJECT = new("JSON101", "Expected start of object, but found {0}.");
        internal static readonly JsonFhirException RESOURCETYPE_SHOULD_BE_STRING = new("JSON102", "Property 'resourceType' should be a string, but found {0}.");
        internal static readonly JsonFhirException NO_RESOURCETYPE_PROPERTY = new("JSON103", "Resource has no 'resourceType' property.");
        internal static readonly JsonFhirException EXPECTED_PRIMITIVE_NOT_OBJECT = new("JSON104", "Expected a primitive value, not a json object.");
        internal static readonly JsonFhirException EXPECTED_PRIMITIVE_NOT_ARRAY = new("JSON105", "Expected a primitive value, not the start of an array.");
        internal static readonly JsonFhirException INCORRECT_BASE64_DATA = new("JSON106", "Encountered incorrectly encoded base64 data.");
        internal static readonly JsonFhirException STRING_ISNOTAN_INSTANT = new("JSON107", "Literal string '{0}' cannot be parsed as an instant.");
        internal static readonly JsonFhirException NUMBER_CANNOT_BE_PARSED = new("JSON108", "Json number '{0}' cannot be parsed as a {1}.");
        internal static readonly JsonFhirException EXPECTED_PRIMITIVE_NOT_NULL = new("JSON109", "Expected a primitive value, not a json null.");
        internal static readonly JsonFhirException UNEXPECTED_JSON_TOKEN = new("JSON110", "Expecting a {0}, but found a json {1} with value '{2}'.");
        internal static readonly JsonFhirException EXPECTED_START_OF_ARRAY = new("JSON111", "Expected start of array since '{0}' is a repeating element.");
        internal static readonly JsonFhirException USE_OF_UNDERSCORE_ILLEGAL = new("JSON113", "Element '{0}' is not a FHIR primitive, so it should not use an underscore in the '{1}' property.");
        internal static readonly JsonFhirException CHOICE_ELEMENT_HAS_NO_TYPE = new("JSON114", "Choice element '{0}' is not suffixed with a type.");
        internal static readonly JsonFhirException CHOICE_ELEMENT_HAS_UNKOWN_TYPE = new("JSON115", "Choice element '{0}' is suffixed with an unrecognized type '{1}'.");
        internal static readonly JsonFhirException UNKNOWN_RESOURCE_TYPE = new("JSON116", "Unknown type '{0}' found in 'resourceType' property.");
        internal static readonly JsonFhirException RESOURCE_TYPE_NOT_A_RESOURCE = new("JSON117", "Data type '{0}' in property 'resourceType' is not a type of resource.");
        internal static readonly JsonFhirException UNKNOWN_PROPERTY_FOUND = new("JSON118", "Encountered unrecognized property '{0}'.");
        internal static readonly JsonFhirException RESOURCETYPE_UNEXPECTED = new("JSON119", "The 'resourceType' property should only be used in resources.");
        internal static readonly JsonFhirException OBJECTS_CANNOT_BE_EMPTY = new("JSON120", "An object needs to have at least one property.");
        internal static readonly JsonFhirException ARRAYS_CANNOT_BE_EMPTY = new("JSON121", "An array needs to have at least one element.");
        internal static readonly JsonFhirException PRIMITIVE_ARRAYS_INCOMPAT_SIZE = new("JSON122", "Primitive arrays split in two properties should have the same size.");
        internal static readonly JsonFhirException PRIMITIVE_ARRAYS_BOTH_NULL = new("JSON123", "Primitive arrays split in two properties should not both have a null at the same position.");
        internal static readonly JsonFhirException PRIMITIVE_ARRAYS_LONELY_NULL = new("JSON124", "Property '{0}' is a single primitive array and should not contain a null.");
        internal static readonly JsonFhirException PRIMITIVE_ARRAYS_ONLY_NULL = new("JSON125", "If present, property '{0}' should not only contain nulls.");
        internal static readonly JsonFhirException INCOMPATIBLE_SIMPLE_VALUE = new("JSON126", "Found a json primitive value that does not match the expected type of the primitive property. Details: {0}");

        /// <summary>
        /// The set of errors that can be considered to not lose data and so can be used to simulate the old "permissive" parsing option.
        /// </summary>
        public static readonly JsonFhirException[] PERMISSIVESET = new[]
        { 
            // The serialization contained a json null where it is not allowed, but a null does not contain data anyway.
            EXPECTED_PRIMITIVE_NOT_NULL,

            // These errors signal parsing errors, but the original raw data is retained in the POCO so no data is lost.
            INCORRECT_BASE64_DATA, STRING_ISNOTAN_INSTANT, NUMBER_CANNOT_BE_PARSED, UNEXPECTED_JSON_TOKEN,

            // The parser will turn a non-array value into an array with a single element, so no data is lost.
            EXPECTED_START_OF_ARRAY,

            // We will just ignore the underscore and keep on parsing
            USE_OF_UNDERSCORE_ILLEGAL,

            // The serialization contained a superfluous 'resourceType' property, but we have read all data anyway.
            RESOURCETYPE_UNEXPECTED,

            // Empty objects and arrays can be ignored without discarding data
            OBJECTS_CANNOT_BE_EMPTY, ARRAYS_CANNOT_BE_EMPTY,

            // Shortest array will be filled out with nulls
            PRIMITIVE_ARRAYS_INCOMPAT_SIZE,
            
            // This leaves the incorrect nulls in place, no change in data.
            PRIMITIVE_ARRAYS_BOTH_NULL, PRIMITIVE_ARRAYS_LONELY_NULL, PRIMITIVE_ARRAYS_ONLY_NULL
        };


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