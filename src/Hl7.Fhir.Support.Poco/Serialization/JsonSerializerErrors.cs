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
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;

#nullable enable

namespace Hl7.Fhir.Serialization
{
    public class JsonFhirException : JsonException
    {
        public string ErrorCode { get; private set; }

        public Base? PartialResult { get; private set; }

        public JsonFhirException(string code, string message) :
            this(code, message, partialResult: null, lineNumber: null, bytePositionInLine: null, innerException: null)
        {
        }

        public JsonFhirException(string code, string message, Base? partialResult) :
            this(code, message, partialResult, lineNumber: null, bytePositionInLine: null, innerException: null)
        {
        }

        public JsonFhirException(string code, string message, Exception? innerException) :
            this(code, message, partialResult: null, lineNumber: null, bytePositionInLine: null, innerException)
        {

        }

        public JsonFhirException(string code, string message, Base? partialResult, Exception? innerException) :
            this(code, message, partialResult, lineNumber: null, bytePositionInLine: null, innerException)
        {

        }

        public JsonFhirException(string code, string message, long? lineNumber, long? bytePositionInLine) :
            this(code, message, partialResult: null, lineNumber, bytePositionInLine, innerException: null)
        {
        }

        public JsonFhirException(string code, string message, Base? partialResult, long? lineNumber, long? bytePositionInLine) :
            this(code, message, partialResult, lineNumber, bytePositionInLine, innerException: null)
        {
        }

        public JsonFhirException(string code, string message, Base? partialResult, long? lineNumber, long? bytePositionInLine, Exception? innerException) : base(message, path: null, lineNumber, bytePositionInLine, innerException)
        {
            ErrorCode = code;
            PartialResult = partialResult;
        }
    }


    public static class JsonSerializerErrors
    {
        public static readonly JsonFhirException JSON101 = new("JSON101", "Expected start of object, but found {0}.");
        public static readonly JsonFhirException JSON102 = new("JSON102", "Property 'resourceType' should be a string, but found {0}.");
        public static readonly JsonFhirException JSON103 = new("JSON103", "Resource has no 'resourceType' property.");
        public static readonly JsonFhirException JSON104 = new("JSON104", "Expected a primitive value, not a json object.");
        public static readonly JsonFhirException JSON105 = new("JSON105", "Expected a primitive value, not the start of an array.");
        public static readonly JsonFhirException JSON106 = new("JSON106", "Encountered incorrectly encoded base64 data.");
        public static readonly JsonFhirException JSON107 = new("JSON107", "Literal string '{0}' cannot be parsed as a {1}.");
        public static readonly JsonFhirException JSON108 = new("JSON108", "Json number '{0}' cannot be parsed as a {1}.");
        public static readonly JsonFhirException JSON109 = new("JSON109", "A json null cannot be used here.");
        public static readonly JsonFhirException JSON110 = new("JSON110", "Expecting a {0}, but found a json {1}.");

        public static readonly JsonFhirException JSON201 = new("JSON201", "Unknown resource type '{0}'.");
        public static readonly JsonFhirException JSON202 = new("JSON202", "Data type '{0}' in property 'resourceType' is not a type of resource.");
        public static readonly JsonFhirException JSON203 = new("JSON203", "Encountered unrecognized property '{0}'.");

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


        internal static JsonFhirException With(this JsonFhirException protoType, ref Utf8JsonReader reader, params object?[] parameters) =>
            With(protoType, ref reader, partialResult: null, innerException: null, parameters);

        internal static JsonFhirException With(this JsonFhirException protoType, ref Utf8JsonReader reader, Base? partialResult, params object?[] parameters) =>
            With(protoType, ref reader, partialResult, innerException: null, parameters);

        internal static JsonFhirException With(this JsonFhirException protoType, ref Utf8JsonReader reader, Base? partialResult, Exception? innerException, params object?[] parameters)
        {
            var formattedMessage = string.Format(protoType.Message, parameters);

            // While we are waiting for this https://github.com/dotnet/runtime/issues/28482,
            // there's no other option than to just force our way to these valuable properties.
            var lineNumber = (long)typeof(JsonReaderState)
                .GetField("_lineNumber", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(reader.CurrentState)!;

            var position = (long)typeof(JsonReaderState)
                .GetField("_bytePositionInLine", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(reader.CurrentState)!;

            var location = $" Line {lineNumber}, position {position}.";
            var message = formattedMessage + location;

            return new JsonFhirException(protoType.ErrorCode, message, partialResult, lineNumber, position, innerException);
        }

    }
}

#nullable restore
#endif