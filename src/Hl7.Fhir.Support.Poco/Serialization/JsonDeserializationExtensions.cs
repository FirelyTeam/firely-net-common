/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using Hl7.Fhir.Model;
using System;
using System.Collections;
using System.Collections.Generic;

#if NETSTANDARD2_0_OR_GREATER
using System.Text.Json;
#endif

#nullable enable


namespace Hl7.Fhir.Serialization
{
#if NETSTANDARD2_0_OR_GREATER

    public class FactoryList<T> : List<T> where T : new()
    {
        public T NewElement() => new();
    }

    public static class JsonDeserializationExtensions
    {
        public static bool TryGetMatchingNumber(this ref Utf8JsonReader reader, Type numbertype, out object? value)
        {
            value = default;

            if (numbertype == typeof(object) || numbertype == typeof(decimal))
                return reader.TryGetDecimal(out decimal dec) && (value = dec) is { };
            else if (numbertype == typeof(int))
                return reader.TryGetInt32(out int i32) && (value = i32) is { };
            else if (numbertype == typeof(uint))
                return reader.TryGetUInt32(out uint ui32) && (value = ui32) is { };
            else if (numbertype == typeof(long))
                return reader.TryGetInt64(out long i64) && (value = i64) is { };
            else if (numbertype == typeof(ulong))
                return reader.TryGetUInt64(out ulong ui64) && (value = ui64) is { };
            else if (numbertype == typeof(float))
                return reader.TryGetSingle(out float si) && (value = si) is { };
            else if (numbertype == typeof(double))
                return reader.TryGetDouble(out double dbl) && (value = dbl) is { };
            else
                return false;
        }

        // TODO: calling function should have figured out how to create the target, i.e. at root by finding the resourceType or simply
        // because the caller of the SDK passes in an instance since the type is known beforehand (i.e. when parsing a subtree).
        // TODO: Assumes the reader is configured to either skip or refuse comments:
        //             reader.CurrentState.Options.CommentHandling is Skip or Disallow
        public static void DeserializeObject(Base target, ref Utf8JsonReader reader)
        {
            // Are these json exceptions of some kind of our own (existing) format/type exceptions?
            // There's formally nothing wrong with the json, so throwing JsonException seems wrong.
            // I think these need to be StructuralTypeExceptions - to align with the current parser.
            // And probably use the same error text too.
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException($"Expected start of object since '{target.TypeName}' is not a primitive, but found {reader.TokenType}.");

            // read past start of object into first property or end of object
            reader.Read();

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                deserializeMember(target, ref reader);
            }

            // read past object
            reader.Read();
        }

        // Reads the content of a json property. Expects the reader to be positioned on the property value.
        // Reader will be on the first token after the property value upon return.
        private static void deserializeMember(Base target, ref Utf8JsonReader reader)
        {
            // TODO: call overload with "createMemberIfMissing: true"
            if (!target.TryGetValue(elementName, out var memberTarget))
                throw new JsonException($"Unknown property {propertyName}.");

            // read past the property name into the value
            reader.Read();

            if (memberTarget is PrimitiveType pt)
                deserializeFhirPrimitive(target, ref reader);
            else if (memberTarget is IEnumerable<PrimitiveType> pts)
                deserializeFhirPrimitiveList(target, ref reader);
            else
            {
                if (memberTarget is IList coll)
                {
                    if (reader.TokenType != JsonTokenType.StartArray)
                        // TODO: need the element name here
                        throw new JsonException($"Expected start of array since '{propertyName}' is a repeating element.");

                    // Read past start of array
                    reader.Read();

                    while (reader.TokenType != JsonTokenType.EndArray)
                    {
                        // TODO: cannot "set" primitive values - need some way to call setter
                        coll.Add(deserializeMemberValue(memberTarget, ref reader));
                    }

                    // Read past end of array
                    reader.Read();
                }
                else
                {
                    target[elementName] = deserializeMemberValue(memberTarget, ref reader);
                }
            }
        }

        private static void deserializeFhirPrimitiveList(Base target, ref Utf8JsonReader reader)
        {
            var propertyName = reader.GetString()!;
            bool isValueProperty = propertyName[0] != '_';
            var elementName = !isValueProperty ? propertyName.Substring(1) : propertyName;

            // read past the property name into the array
            reader.Read();

            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException($"Expected start of array since '{elementName}' is a repeating element.");

            // read into array
            reader.Read();

            bool existed = target.TryGetValue(elementName, out object existing);

            // TODO actually create a new instance of this list.
            IReadOnlyList<PrimitiveType> listItems = existed && (existing is List<PrimitiveType> coll) ? coll : new List<FhirBoolean>();

            // TODO: We can speed this up by having a codepath for adding to existing items,
            // and having a fresh (yield based) factory returning an IEnumerable and then initializing
            // a new list with this IEnumerable.
            int elementIndex = 0;
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (elementIndex >= listItems.Count)
                {
                    if (existed)
                    {
                        // check, if the property already existed, whether the # of new items
                        // is the same as the number of old items to make sure 'element' and '_element' agree.
                        var propWithoutUnderscore = propertyName.TrimStart('_');
                        throw new JsonException($"Number of items at {propWithoutUnderscore} should agree with property _{propWithoutUnderscore}.");
                    }
                    else
                        ((IList)listItems).Add(new FhirBoolean());
                }

                if (isValueProperty)
                {
                    listItems[elementIndex].ObjectValue = deserializePrimitiveValue(ref reader, typeof(bool));
                }
                else
                {
                    DeserializeObject(listItems[elementIndex], ref reader);
                }

                elementIndex += 1;
            }

            reader.Read();
        }

        private static void deserializeFhirPrimitive(Base target, ref Utf8JsonReader reader) => throw new NotImplementedException();

        private static object deserializeMemberValue(object target, ref Utf8JsonReader reader)
        {
            if (target is Base complex)
            {
                DeserializeObject(complex, ref reader);
                return complex;
            }
            else
            {
                // FHIR serialization does not allow `null` to be used in normal property values.
                return deserializePrimitiveValue(ref reader, target.GetType())
                    ?? throw new JsonException("Null cannot be used here.");
            }
        }

        // NB: requiredType can be object (and will be for most PrimitiveType.ObjectValue), which means basically no
        // specific required type. This can be used to implement "lenient" treatment of primitive values where the
        // target model can contain invalid values.
        private static object? deserializePrimitiveValue(ref Utf8JsonReader reader, Type requiredType)
        {
            var result = reader.TokenType switch
            {
                JsonTokenType.StartObject => throw new JsonException("Expected a primitive value, not an object."),
                JsonTokenType.StartArray => throw new JsonException("Expected a primitive value, not the start of an array."),
                JsonTokenType.String when requiredType == typeof(object) || requiredType == typeof(string) =>
                    reader.GetString(),
                //TODO: catch parse errors in the next two cases
                JsonTokenType.String when requiredType == typeof(byte[]) =>
                    reader.GetBytesFromBase64(),
                JsonTokenType.String when requiredType == typeof(DateTimeOffset) =>
                    // TODO: Make sure the precision is right so there cannot be a missing timezone - verify behaviour of current parser
                    ElementModel.Types.DateTime.Parse(reader.GetString()!).ToDateTimeOffset(TimeSpan.Zero),
                JsonTokenType.String => throw new JsonException($"Expecting a {requiredType}, but found a string."),
                JsonTokenType.Number => reader.TryGetMatchingNumber(requiredType, out var numberValue)
                    ? numberValue
                    : throw new JsonException($"Cannot parse number '{reader.GetDecimal()}' into a {requiredType}."),
                JsonTokenType.True or JsonTokenType.False => requiredType == typeof(object) || requiredType == typeof(bool)
                    ? reader.GetBoolean()
                    : throw new JsonException($"Expecting a {requiredType}, but found a boolean."),
                JsonTokenType.Null => null,
                _ =>
                    // This would be an internal logic error, since our callers should have made sure we're
                    // on the value after the property name (and the Utf8JsonReader would have complained about any
                    // other token that one that is a value).
                    throw new InvalidOperationException($"Unexpected token type {reader.TokenType}."),
            };

            // Read past the value
            reader.Read();

            return result;
        }
    }
#endif
}

#nullable restore
