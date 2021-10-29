/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */


#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;

#nullable enable

namespace Hl7.Fhir.Serialization
{
    public class DeserializationFailedException : AggregateException
    {
        public static DeserializationFailedException Create(Base? partialResult, Exception exception)
        {        
            if (exception is AggregateException ae)
            {
                var children = ae.Flatten().InnerExceptions;
                //var message = $"Deserialization failed with {children.Count} errors.";
                var message = ae.Message;
                return new DeserializationFailedException(message, partialResult, children);
            }
            else
            {
                var message = "Deserialization failed with one error: " + exception.Message;
                return new DeserializationFailedException(message, partialResult, new[] { exception });
            }
        }

        private DeserializationFailedException(string message, Base? partialResult, IEnumerable<Exception> innerExceptions) : base(message, innerExceptions)
        {
            PartialResult = partialResult;
        }

        public Base? PartialResult { get; private set; }
    }

    public class JsonFhirException : JsonException
    {
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
    }


    public static class JsonSerializerErrors
    {
        internal static readonly JsonFhirException EXPECTED_START_OF_OBJECT         = new("JSON101", "Expected start of object, but found {0}.");
        internal static readonly JsonFhirException RESOURCETYPE_SHOULD_BE_STRING    = new("JSON102", "Property 'resourceType' should be a string, but found {0}.");
        internal static readonly JsonFhirException NO_RESOURCETYPE_PROPERTY         = new("JSON103", "Resource has no 'resourceType' property.");
        internal static readonly JsonFhirException EXPECTED_PRIMITIVE_NOT_OBJECT    = new("JSON104", "Expected a primitive value, not a json object.");
        internal static readonly JsonFhirException EXPECTED_PRIMITIVE_NOT_ARRAY     = new("JSON105", "Expected a primitive value, not the start of an array.");
        internal static readonly JsonFhirException EXPECTED_PRIMITIVE_NOT_NULL      = new("JSON109", "Expected a primitive value, not a json null.");
        internal static readonly JsonFhirException INCORRECT_BASE64_DATA            = new("JSON106", "Encountered incorrectly encoded base64 data.");
        internal static readonly JsonFhirException STRING_ISNOTA_DATETIME           = new("JSON107", "Literal string '{0}' cannot be parsed as a datetime.");
        internal static readonly JsonFhirException NUMBER_CANNOT_BE_PARSED          = new("JSON108", "Json number '{0}' cannot be parsed as a {1}.");
        internal static readonly JsonFhirException UNEXPECTED_JSON_TOKEN            = new("JSON110", "Expecting a {0}, but found a json {1}.");
        internal static readonly JsonFhirException EXPECTED_START_OF_ARRAY          = new("JSON111", "Expected start of array since '{0}' is a repeating element.");
        internal static readonly JsonFhirException START_OF_ARRAY_UNEXPECTED        = new("JSON112", "Found the start of an array, but '{0}' is not a repeating element.");
        internal static readonly JsonFhirException USE_OF_UNDERSCORE_ILLEGAL       = new("JSON113", "Element '{0}' is not a FHIR primitive, so it should not use an underscore in the '{1}' property.");
        internal static readonly JsonFhirException CHOICE_ELEMENT_HAS_NO_TYPE       = new("JSON114", "Choice element '{0}' is not suffixed with a type.");
        internal static readonly JsonFhirException CHOICE_ELEMENT_HAS_UNKOWN_TYPE   = new("JSON115", "Choice element '{0}' is suffixed with an unrecognized type '{1}'.");
        internal static readonly JsonFhirException UNKNOWN_RESOURCE_TYPE            = new("JSON116", "Unknown type '{0}' found in 'resourceType' property.");
        internal static readonly JsonFhirException EXPECTED_A_RESOURCE_TYPE         = new("JSON117", "Data type '{0}' in property 'resourceType' is not a type of resource.");
        internal static readonly JsonFhirException UNKNOWN_PROPERTY_FOUND           = new("JSON118", "Encountered unrecognized property '{0}'.");
        internal static readonly JsonFhirException RESOURCETYPE_UNEXPECTED_IN_DT    = new("JSON119", "The 'resourceType' property should only be used in resources.");
        internal static readonly JsonFhirException OBJECTS_CANNOT_BE_EMPTY          = new("JSON120", "An object needs to have at least one property.");
        internal static readonly JsonFhirException ARRAYS_CANNOT_BE_EMPTY           = new("JSON121", "An array needs to have at least one element.");
        internal static readonly JsonFhirException PRIMITIVE_ARRAYS_INCOMPAT_SIZE   = new("JSON122", "Primitive arrays split in two properties should have the same size.");
        internal static readonly JsonFhirException PRIMITIVE_ARRAYS_BOTH_NULL       = new("JSON123", "Primitive arrays split in two properties should not both have a null at the same position.");
        internal static readonly JsonFhirException PRIMITIVE_ARRAYS_LONELY_NULL     = new("JSON123", "Property '{0}' is a single primitive array and should not contain a null.");

        /// <summary>
        /// The set of errors that can be considered to not lose data and so can be used to simulate the old "permissive" parsing option.
        /// </summary>
        public static readonly JsonFhirException[] PERMISSIVESET = new []
        { 
            // These errors signal parsing errors, but the original raw data is retained in the POCO so no data is lost.
            INCORRECT_BASE64_DATA, STRING_ISNOTA_DATETIME, NUMBER_CANNOT_BE_PARSED, UNEXPECTED_JSON_TOKEN,

            // The serialization contained a json null where it is not allowed, but a null does not contain data anyway.
            EXPECTED_PRIMITIVE_NOT_NULL,

            // We will just ignore the underscore and keep on parsing
            USE_OF_UNDERSCORE_ILLEGAL,

            // We expected a resource, but found another datatype. Regardless, we will return all data.
            EXPECTED_A_RESOURCE_TYPE,

            // The serialization contained a superfluous 'resourceType' property, but we have read all data anyway.
            RESOURCETYPE_UNEXPECTED_IN_DT,

            // Empty objects and arrays can be ignored without discarding data
            OBJECTS_CANNOT_BE_EMPTY, ARRAYS_CANNOT_BE_EMPTY,

            PRIMITIVE_ARRAYS_INCOMPAT_SIZE, PRIMITIVE_ARRAYS_BOTH_NULL, PRIMITIVE_ARRAYS_LONELY_NULL
        };

        internal static JsonFhirException With(this JsonFhirException protoType, ref Utf8JsonReader reader, params object?[] parameters)
        {
            var formattedMessage = string.Format(protoType.Message, parameters);

            var location = GenerateLocationMessage(ref reader, out var lineNumber, out var position);
            var message = formattedMessage + location;

            return new JsonFhirException(protoType.ErrorCode, message, lineNumber, position);
        }

        internal static string GenerateLocationMessage(ref Utf8JsonReader reader) =>
            GenerateLocationMessage(ref reader, out var _, out var _);

        internal static string GenerateLocationMessage(ref Utf8JsonReader reader, out long lineNumber, out long position)
        {
            // While we are waiting for this https://github.com/dotnet/runtime/issues/28482,
            // there's no other option than to just force our way to these valuable properties.
            lineNumber = (long)typeof(JsonReaderState)
                .GetField("_lineNumber", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(reader.CurrentState)!;
            position = (long)typeof(JsonReaderState)
                .GetField("_bytePositionInLine", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(reader.CurrentState)!;

            return $" Line {lineNumber}, position {position}.";
        }
    }
}

#nullable restore
#endif