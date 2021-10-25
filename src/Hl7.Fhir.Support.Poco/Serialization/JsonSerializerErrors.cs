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
                var message = $"Deserialization failed with {children.Count} errors.";
                return new DeserializationFailedException(message, partialResult, children);
            }
            else
            {
                var message = "Deserialization failed with one error.";
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
        internal static readonly JsonFhirException JSON101 = new("JSON101", "Expected start of object, but found {0}.");
        internal static readonly JsonFhirException JSON102 = new("JSON102", "Property 'resourceType' should be a string, but found {0}.");
        internal static readonly JsonFhirException JSON103 = new("JSON103", "Resource has no 'resourceType' property.");
        internal static readonly JsonFhirException JSON104 = new("JSON104", "Expected a primitive value, not a json object.");
        internal static readonly JsonFhirException JSON105 = new("JSON105", "Expected a primitive value, not the start of an array.");
        internal static readonly JsonFhirException JSON106 = new("JSON106", "Encountered incorrectly encoded base64 data.");
        internal static readonly JsonFhirException JSON107 = new("JSON107", "Literal string '{0}' cannot be parsed as a datetime.");
        internal static readonly JsonFhirException JSON108 = new("JSON108", "Json number '{0}' cannot be parsed as a {1}.");
        internal static readonly JsonFhirException JSON109 = new("JSON109", "A json null cannot be used here.");
        internal static readonly JsonFhirException JSON110 = new("JSON110", "Expecting a {0}, but found a json {1}.");
        internal static readonly JsonFhirException JSON111 = new("JSON111", "Expected start of array since '{0}' is a repeating element.");
        internal static readonly JsonFhirException JSON112 = new("JSON112", "Found the start of an array, but '{0}' is not a repeating element.");
        internal static readonly JsonFhirException JSON113 = new("JSON113", "Element '{0}' is not a FHIR primitive, so it should not use an underscore in the '{1}' property.");
        internal static readonly JsonFhirException JSON114 = new("JSON114", "Choice element '{0}' is not suffixed with a type.");
        internal static readonly JsonFhirException JSON115 = new("JSON115", "Choice element '{0}' is suffixed with an unrecognized type '{1}'.");
        internal static readonly JsonFhirException JSON201 = new("JSON201", "Unknown resource type '{0}'.");
        internal static readonly JsonFhirException JSON202 = new("JSON202", "Data type '{0}' in property 'resourceType' is not a type of resource.");
        internal static readonly JsonFhirException JSON203 = new("JSON203", "Encountered unrecognized property '{0}'.");

        /// <summary>
        /// The set of errors that can be considered to not lose data and so can be used to simulate the old "permissive" parsing option.
        /// </summary>
        public static readonly string[] PERMISSIVESET = new string[]
        { 
            // These errors signal parsing errors, but the original raw data is retained in the POCO so no data is lost.
            JSON106.ErrorCode, JSON107.ErrorCode, JSON108.ErrorCode, JSON110.ErrorCode,

            // The serialization contained a json null where it is not allowed, but a null does not contain data anyway.
            JSON109.ErrorCode,
        };

        internal static JsonFhirException With(this JsonFhirException protoType, ref Utf8JsonReader reader, params object?[] parameters)
        {
            var formattedMessage = string.Format(protoType.Message, parameters);

            var location = GenerateLocationMessage(ref reader, out var lineNumber, out var position);
            var message = formattedMessage + location;

            return new JsonFhirException(protoType.ErrorCode, message, lineNumber, position);
        }

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