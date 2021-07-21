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
    public class JsonDictionarySerializer
    {
        public static bool HasValue(object? value) =>
             value switch
             {
                 null => false,
                 string s => !string.IsNullOrWhiteSpace(s),
                 byte[] bs => bs.Length > 0,
                 _ => true
             };

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

        public void SerializeObject(IReadOnlyDictionary<string, object> members, Utf8JsonWriter writer) => SerializeObject(members, skipValue: false, writer);

        private void serializeMemberValue(object value, Utf8JsonWriter writer)
        {
            if (value is IReadOnlyDictionary<string, object> complex)
                SerializeObject(complex, writer);
            else
                SerializePrimitiveValue(value, writer);
        }

        // TODO: There's lots of HasValue() everywhere..... can we make the IDictionary implementation promise not to
        // return kvp's unless there is actually a value?  What if the IDictionary is constructed by hand?  Should
        // the IDictionary implementation on POCOs worry about the special cases (empty strings etc), and this serialize
        // be more generic - just not serializing nulls, but follow the IDictionary otherwise (even it that returns empty
        // strings?).
        public void SerializeFhirPrimitiveList(string elementName, IReadOnlyCollection<PrimitiveType> values, Utf8JsonWriter writer)
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


        public void SerializeFhirPrimitive(string elementName, PrimitiveType value, Utf8JsonWriter writer)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            if (HasValue(value.ObjectValue))
            {
                // Write a property with 'elementName'
                writer.WritePropertyName(elementName);
                SerializePrimitiveValue(value.ObjectValue, writer);
            }

            // TODO: Should the POCO types explicitly or implicitly implement these interfaces?
            // TODO: Implicitly and then have a AsDictionary() and AsEnumerable()?

            if (value.HasElements)
            {
                // Write a property with '_elementName'
                writer.WritePropertyName("_" + elementName);
                SerializeObject(value, skipValue: true, writer);
            }
        }

        public void SerializePrimitiveValue(object value, Utf8JsonWriter writer)
        {
            switch (value)
            {
                //TODO: Include support for Any subclasses (CQL types)?
                //TODO: precision loss in WriteNumber/ParseNumber (http://hl7.org/fhir/json.html#primitive)?
                case int i32: writer.WriteNumberValue(i32); break;
                case uint ui32: writer.WriteNumberValue(ui32); break;
                case long i64: writer.WriteNumberValue(i64); break;
                case ulong ui64: writer.WriteNumberValue(ui64); break;
                case float si: writer.WriteNumberValue(si); break;
                case double dbl: writer.WriteNumberValue(dbl); break;
                case decimal dec: writer.WriteNumberValue(dec); break;
                // So, no trim here. string-based types (like code) should make sure their values are valid,
                // so do not have trailing spaces. Not something the serializer should worry about? And strings
                // are allowed to have trailing spaces:
                // "According to XML schema, leading and trailing whitespace in the value attribute is ignored for the
                // types boolean, integer, decimal, base64Binary, instant, uri, date, dateTime, oid, and uri. Note that
                // this means that the schema aware XML libraries give different attribute values to non-schema aware libraries
                // when reading the XML instances. For this reason, the value attribute for these types SHOULD not have leading
                // and trailing spaces. String values should only have leading and trailing spaces if they are part of the content
                // of the value. In JSON and Turtle whitespace in string values is always significant. Primitive types other than
                // string SHALL NOT have leading or trailing whitespace."
                case string s: writer.WriteStringValue(s); break;
                case bool b: writer.WriteBooleanValue(b); break;
                //TODO: only two types that do not support 100% roundtrippability. Currently necessary for Instant.cs
                // change Instant to have an ObjectValue of type string?  But then again, why is 'bool' good enough for Boolean,
                // wouldn't we need to be able to store "treu" or "flase" for real roundtrippability?
                case DateTimeOffset dto: writer.WriteStringValue(ElementModel.Types.DateTime.FormatDateTimeOffset(dto)); break;
                case byte[] bytes: writer.WriteStringValue(Convert.ToBase64String(bytes)); break;
                case null: writer.WriteNullValue(); break;
                default:
                    throw new FormatException($"There is no know serialization for type {value.GetType()} into Json.");
            }
        }
    }
}

#endif
#nullable restore
