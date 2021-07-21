/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

#nullable enable

#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER

using Hl7.Fhir.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;

namespace Hl7.Fhir.Serialization
{
    /// <summary>
    /// 
    /// </summary>
    public class JsonDictionarySerializer
    {
        /// <summary>
        /// Serializes the given dictionary with FHIR data into Json.
        /// </summary>
        /// <remarks>The serializer uses the format documented in https://www.hl7.org/fhir/json.html.
        /// </remarks>
        public void SerializeObject(IReadOnlyDictionary<string, object> members, Utf8JsonWriter writer) => SerializeObject(members, skipValue: false, writer);

        // TODO: prune empty subtrees
        internal void SerializeObject(IReadOnlyDictionary<string, object> members, bool skipValue, Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            foreach (var member in members)
            {
                if (skipValue && member.Key == "value") continue;

                if (member.Value is PrimitiveType pt)
                    SerializeFhirPrimitive(member.Key, pt, writer);
                else if (member.Value is IReadOnlyCollection<PrimitiveType> pts)
                    SerializeFhirPrimitiveList(member.Key, pts, writer);
                else
                {
                    writer.WritePropertyName(member.Key);

                    if (member.Value is ICollection coll && !(member.Value is byte[]))
                    {
                        writer.WriteStartArray();

                        foreach (var value in coll)
                            serializeMemberValue(value, writer);

                        writer.WriteEndArray();
                    }
                    else
                        serializeMemberValue(member.Value, writer);
                }
            }

            writer.WriteEndObject();
        }

        internal static bool HasValue(object? value) =>
             value switch
             {
                 null => false,
                 string s => !string.IsNullOrWhiteSpace(s),
                 byte[] bs => bs.Length > 0,
                 _ => true
             };

        private void serializeMemberValue(object value, Utf8JsonWriter writer)
        {
            if (value is IReadOnlyDictionary<string, object> complex)
                SerializeObject(complex, writer);
            else
                SerializePrimitiveValue(value, writer);
        }

        internal void SerializeFhirPrimitiveList(string elementName, IReadOnlyCollection<PrimitiveType> values, Utf8JsonWriter writer)
        {
            if (values is null) throw new ArgumentNullException(nameof(values));

            // Don't serialize empty collections.
            if (values.Count == 0) return;

            // We should not write a "elementName" property until we encounter an actual
            // value. If we do, we should "catch up", by creating the property starting 
            // with a json array that contains 'null' for each of the elements we encountered
            // until now that did not have a value id/extensions.
            bool wroteStartArray = false;
            int numNullsMissed = 0;

            foreach (var value in values)
            {
                if (HasValue(value?.ObjectValue))
                {
                    if (!wroteStartArray)
                    {
                        wroteStartArray = true;
                        writeStartArray(elementName, numNullsMissed, writer);
                    }

                    SerializePrimitiveValue(value!.ObjectValue, writer);
                }
                else
                {
                    if (wroteStartArray)
                        writer.WriteNullValue();
                    else
                        numNullsMissed += 1;
                }
            }

            if (wroteStartArray) writer.WriteEndArray();

            // We should not write a "_elementName" property until we encounter an actual
            // id/extension. If we do, we should "catch up", by creating the property starting 
            // with a json array that contains 'null' for each of the elements we encountered
            // until now that did not have id/extensions etc.
            wroteStartArray = false;
            numNullsMissed = 0;

            foreach (var value in values)
            {
                if (value.HasElements)
                {
                    if (!wroteStartArray)
                    {
                        wroteStartArray = true;
                        writeStartArray("_" + elementName, numNullsMissed, writer);
                    }

                    SerializeObject(value, skipValue: true, writer);
                }
                else
                {
                    if (wroteStartArray)
                        writer.WriteNullValue();
                    else
                        numNullsMissed += 1;
                }
            }

            if (wroteStartArray) writer.WriteEndArray();
        }

        private void writeStartArray(string propName, int numNulls, Utf8JsonWriter writer)
        {
            writer.WriteStartArray(propName);

            for (int i = 0; i < numNulls; i++)
                writer.WriteNullValue();
        }


        internal void SerializeFhirPrimitive(string elementName, PrimitiveType value, Utf8JsonWriter writer)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            if (HasValue(value.ObjectValue))
            {
                // Write a property with 'elementName'
                writer.WritePropertyName(elementName);
                SerializePrimitiveValue(value.ObjectValue, writer);
            }

            if (value.HasElements)
            {
                // Write a property with '_elementName'
                writer.WritePropertyName("_" + elementName);
                SerializeObject(value, skipValue: true, writer);
            }
        }

        /// <summary>
        /// Serialize a primitive .NET value that may occur in the POCOs into Json.
        /// </summary>
        /// <remarks>      
        /// To allow for future additions to the POCOs the list of primitives supported here
        /// is larger than the set used by the current POCOs. Note that <c>DateTimeOffset</c>c> and 
        /// <c>byte[]</c> are considered to be "primitive" values here (used as the value in
        /// <see cref="Instant"/> and <see cref="Base64Binary"/>).
        /// 
        /// Note that the current version of System.Text.Json only allows numbers
        /// to be written that fit in .NET's <see cref="decimal"/> type, which may be less 
        /// precision than required by the FHIR specification (http://hl7.org/fhir/json.html#primitive).
        /// </remarks>
        public void SerializePrimitiveValue(object value, Utf8JsonWriter writer)
        {
            switch (value)
            {
                //TODO: Include support for Any subclasses (CQL types)?
                case int i32: writer.WriteNumberValue(i32); break;
                case uint ui32: writer.WriteNumberValue(ui32); break;
                case long i64: writer.WriteNumberValue(i64); break;
                case ulong ui64: writer.WriteNumberValue(ui64); break;
                case float si: writer.WriteNumberValue(si); break;
                case double dbl: writer.WriteNumberValue(dbl); break;
                case decimal dec: writer.WriteNumberValue(dec); break;
                // A little note about trimming and whitespaces. The spec says:
                // "(...) In JSON and Turtle whitespace in string values is always significant. Primitive types other than
                // string SHALL NOT have leading or trailing whitespace."
                // Based on this, we are not trimming whitespace here. Validation is not a part of the responsibilities of
                // the serializer, and string-based types (like code and uri) should make sure their values are valid,
                // so should not have trailing spaces to begin with. strings are allowed to have trailing spaces, but should
                // not just be spaces. The serializer will, however, not serialize an element with only whitespace
                // (or an empty byte[]).
                case string s: writer.WriteStringValue(s); break;
                case bool b: writer.WriteBooleanValue(b); break;
                case DateTimeOffset dto: writer.WriteStringValue(ElementModel.Types.DateTime.FormatDateTimeOffset(dto)); break;
                case byte[] bytes: writer.WriteStringValue(Convert.ToBase64String(bytes)); break;
                case null: writer.WriteNullValue(); break;
                default:
                    throw new FormatException($"There is no know serialization for type {value.GetType()} into a Json primitive property value.");
            }
        }
    }
}

#endif
#nullable restore
