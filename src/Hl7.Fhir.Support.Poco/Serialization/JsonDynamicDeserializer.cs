/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */


#if NETSTANDARD2_0_OR_GREATER
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
#endif

#nullable enable

namespace Hl7.Fhir.Serialization
{
#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    public class JsonDynamicDeserializer
    {
        public JsonDynamicDeserializer(Assembly assembly)
        {
            Assembly = assembly ?? throw new System.ArgumentNullException(nameof(assembly));
            _inspector = ModelInspector.ForAssembly(assembly);
        }

        public Assembly Assembly { get; }

        private readonly ModelInspector _inspector;

        public Resource DeserializeResource(ref Utf8JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.None) reader.Read();

            //TODO: determineResourceType probably won't work with streaming inputs to Utf8JsonReader
            var resourceType = determineResourceType(ref reader);
            var resourceMapping = _inspector.FindClassMapping(resourceType) ??
                throw new JsonException($"Unknown resource type '{resourceType}'.");

            if (resourceMapping.Factory() is Resource resource)
            {
                DeserializeObjectInto(resource, resourceMapping, ref reader, allowNull: false);
                return resource;
            }
            else
                throw new JsonException($"Data type '{resourceType}' found in property 'resourceType' is not a resource.");
        }

        public T DeserializeObject<T>(ref Utf8JsonReader reader) where T : Base => (T)DeserializeObject(typeof(T), ref reader);

        public Base DeserializeObject(Type targetType, ref Utf8JsonReader reader)
        {
            var mapping = SerializationUtilities.FindClassMapping(_inspector, targetType) ??
                throw new ArgumentException($"Type '{targetType}' could not be located in model assembly '{Assembly}' and can therefore not be used for deserialization.", nameof(targetType));

            // Create a new instance of the object to read the members into.
            if (mapping.Factory() is Base b)
            {
                DeserializeObjectInto(b, mapping, ref reader, allowNull: false);
                return b;
            }
            else
                throw new ArgumentException($"Can only deserialize into subclasses of class {nameof(Base)}.", nameof(targetType));
        }

        public void DeserializeObjectInto(object target, ref Utf8JsonReader reader)
        {
            var targetType = target.GetType();
            var mapping = SerializationUtilities.FindClassMapping(_inspector, targetType) ??
                throw new ArgumentException($"Type '{targetType}' could not be located in model assembly '{Assembly}' and can therefore not be used for deserialization.", nameof(targetType));

            DeserializeObjectInto(target, mapping, ref reader, allowNull: false);
        }

        // TODO: Assumes the reader is configured to either skip or refuse comments:
        //             reader.CurrentState.Options.CommentHandling is Skip or Disallow
        internal void DeserializeObjectInto(object target, ClassMapping mapping, ref Utf8JsonReader reader, bool allowNull)
        {
            // Read() will either work, or throw an exception that the json string is empty - which is fine.
            if (reader.TokenType == JsonTokenType.None) reader.Read();

            if (reader.TokenType == JsonTokenType.Null)
            {
                if (allowNull)
                {
                    reader.Read();
                    return;
                }
                else
                    throw new JsonException("Null is not a valid value for an object here.");
            }

            // TODO: Are these json exceptions of some kind of our own (existing) format/type exceptions?
            // There's formally nothing wrong with the json, so throwing JsonException seems wrong.
            // I think these need to be StructuralTypeExceptions - to align with the current parser.
            // And probably use the same error text too.
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException($"Expected start of object since, but found {reader.TokenType}.");

            // read past start of object into first property or end of object
            reader.Read();

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                var currentPropertyName = reader.GetString()!;
                if (currentPropertyName == "resourceType")
                {
                    reader.Read(); // into value
                    reader.Read(); // into next
                    continue;
                }

                // determine the expected type of the value for this property
                var (propMapping, propValueMapping) = SerializationUtilities.GetMappedElementMetadata(_inspector, mapping, currentPropertyName);

                // read past the property name into the value
                reader.Read();

                deserializeMemberValue(target, currentPropertyName, propMapping, propValueMapping, ref reader);
            }

            // read past object
            reader.Read();
        }

        // Reads the content of a json property. Expects the reader to be positioned on the property value.
        // Reader will be on the first token after the property value upon return.
        private void deserializeMemberValue(object target, string propertyName, PropertyMapping propertyMapping, ClassMapping propertyValueMapping, ref Utf8JsonReader reader)
        {
            if (propertyValueMapping.IsFhirPrimitive)
            {
                // There might be an existing value, since FhirPrimitives may be spread out over two properties
                var existing = propertyMapping.GetValue(target);

                if (propertyMapping.IsCollection)
                    propertyMapping.SetValue(target, deserializeFhirPrimitiveList(existing as IList, propertyName, propertyValueMapping, ref reader));
                else
                    propertyMapping.SetValue(target, deserializeFhirPrimitive(existing as PrimitiveType, propertyName, propertyValueMapping, ref reader));
            }
            else
            {
                // This is not a FHIR primitive, so we should not be dealing with these weird _name members.
                if (propertyName[0] == '_')
                    throw new JsonException($"Element '{propertyMapping.Name}' is not a FHIR primitive, so it should not use a '{propertyName}' property.");


                if (propertyMapping.IsCollection)
                {
                    if (reader.TokenType != JsonTokenType.StartArray)
                        throw new JsonException($"Expected start of array since '{propertyName}' is a repeating element.");

                    // Read past start of array
                    reader.Read();

                    // Create a list of the type of this property's value.
                    var listInstance = propertyValueMapping.ListFactory();

                    // TODO: Adding this one by one is probably much slower than yielding and creating a new list
                    // using the IEnumerable constructor or maybe even using AddRange().
                    while (reader.TokenType != JsonTokenType.EndArray)
                        listInstance.Add(deserializeMemberValue(ref reader, propertyValueMapping));

                    propertyMapping.SetValue(target, listInstance);

                    // Read past end of array
                    reader.Read();
                }
                else
                {
                    propertyMapping.SetValue(target, deserializeMemberValue(ref reader, propertyValueMapping));
                }
            }
        }

        private object deserializeMemberValue(ref Utf8JsonReader reader, ClassMapping propertyValueMapping)
        {
            // Resources
            if (propertyValueMapping.IsResource)
            {
                var resource = DeserializeResource(ref reader);
                return propertyValueMapping.NativeType.IsAssignableFrom(resource.GetType())
                    ? resource
                    : throw new InvalidOperationException($"Found a resource of type '{resource.GetType()}', but expected a '{propertyValueMapping.NativeType}'.");
            }

            // primitive values (not FHIR primitives, real primitives, like Element.id)
            else if (propertyValueMapping.IsPrimitive)
            {
                // FHIR serialization does not allow `null` to be used in normal property values.
                return deserializePrimitiveValue(ref reader, propertyValueMapping.NativeType, allowNull: false)!;
            }

            // "normal" complex types & backbones
            else
            {
                var target = propertyValueMapping.Factory();
                DeserializeObjectInto(target, propertyValueMapping, ref reader, allowNull: false);
                return target;
            }
        }

        private static string determineResourceType(ref Utf8JsonReader reader)
        {
            var originalReader = reader;    // copy the struct so we can "rewind"
            var atDepth = reader.CurrentDepth + 1;

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException($"Expected start of object since, but found {reader.TokenType}.");

            try
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.PropertyName && reader.CurrentDepth == atDepth)
                    {
                        var propName = reader.GetString();

                        if (propName == "resourceType")
                        {
                            reader.Read();
                            if (reader.TokenType != JsonTokenType.String)
                                throw new JsonException($"Error reading 'resourceType': expected String, found {reader.TokenType}! depth: {reader.CurrentDepth}, pos: {reader.BytesConsumed}");

                            return reader.GetString()!;
                        }
                    }
                }

                throw new JsonException("Resource does not contain a '_resourceType' property.");
            }
            finally
            {
                reader = originalReader;
            }
        }


        private object? deserializePrimitiveValue(ref Utf8JsonReader reader, Type requiredType, bool allowNull)
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
                JsonTokenType.Number => TryGetMatchingNumber(ref reader, requiredType, out var numberValue)
                    ? numberValue
                    : throw new JsonException($"Cannot parse number '{reader.GetDecimal()}' into a {requiredType}."),
                JsonTokenType.True or JsonTokenType.False => requiredType == typeof(object) || requiredType == typeof(bool)
                    ? reader.GetBoolean()
                    : throw new JsonException($"Expecting a {requiredType}, but found a boolean."),
                JsonTokenType.Null when allowNull => null,
                JsonTokenType.Null => throw new JsonException("Null cannot be used as a primitive value here."),
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

        // NB: requiredType can be object (and will be for most PrimitiveType.ObjectValue), which means basically no
        // specific required type. This can be used to implement "lenient" treatment of primitive values where the
        // target model can contain invalid values.
        public bool TryGetMatchingNumber(ref Utf8JsonReader reader, Type numbertype, out object? value)
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

        private IList deserializeFhirPrimitiveList(IList? existingPrimitiveList, string propertyName, ClassMapping propertyValueMapping, ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException($"Expected start of array since '{propertyName}' is a repeating element.");

            // read into array
            reader.Read();

            var existed = existingPrimitiveList is not null;
            var resultList = existingPrimitiveList ??
                propertyValueMapping.ListFactory() ??
                throw new ArgumentException($"Type of property '{propertyName}' should be a subtype of IList<PrimitiveType>.", nameof(propertyValueMapping));

            // TODO: We can speed this up by having a codepath for adding to existing items,
            // and having a fresh (yield based) factory returning an IEnumerable and then initializing
            // a new list with this IEnumerable.
            int elementIndex = 0;
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (elementIndex >= resultList.Count)
                {
                    //TODO: not an empty array
                    //TODO: not an array with just nulls
                    //if (existed)
                    //{
                    //    // check, if the property already existed, whether the # of new items
                    //    // is the same as the number of old items to make sure 'element' and '_element' agree.
                    //    var nameWithoutUnderscore = propertyName.TrimStart('_');
                    //    throw new JsonException($"Number of items at {nameWithoutUnderscore} should agree with property _{nameWithoutUnderscore}.");
                    //}
                    //else
                    resultList.Add((PrimitiveType)propertyValueMapping.Factory());
                }

                var targetPrimitive = (PrimitiveType)resultList[elementIndex];

                if (propertyName[0] != '_')
                    targetPrimitive.ObjectValue = deserializePrimitiveValue(ref reader, typeof(object), allowNull: true);
                else
                    DeserializeObjectInto(targetPrimitive, propertyValueMapping, ref reader, allowNull: true);

                elementIndex += 1;
            }

            // read past array to next property or end of object
            reader.Read();

            return resultList;
        }

        private PrimitiveType deserializeFhirPrimitive(PrimitiveType? existingPrimitive, string propertyName, ClassMapping propertyValueMapping, ref Utf8JsonReader reader)
        {
            var resultPrimitive = existingPrimitive ??
                propertyValueMapping.Factory() as PrimitiveType ??
                throw new ArgumentException($"Type of property '{propertyName}' should be a subtype of PrimitiveType.", nameof(propertyValueMapping));

            if (propertyName[0] != '_')
                resultPrimitive.ObjectValue = deserializePrimitiveValue(ref reader, typeof(object), allowNull: false);
            else
                DeserializeObjectInto(resultPrimitive, propertyValueMapping, ref reader, allowNull: false);

            return resultPrimitive;
        }
    }


    internal class SerializationUtilities
    {
        public static ClassMapping? FindClassMapping(ModelInspector inspector, Type nativeType) =>
            inspector.FindClassMapping(nativeType) ?? inspector.ImportType(nativeType);
        //nativeType.IsGenericType && nativeType.GetGenericTypeDefinition() == typeof(Code<>)
        //    ? inspector.ImportType(nativeType)
        //    : inspector.FindClassMapping(nativeType);

        /// <summary>
        /// Given a possibly suffixed property name (as encountered in the serialized form), lookup the
        /// mapping for the property and the mapping for the value of the property.
        /// </summary>
        /// <remarks>In case the name is a choice type, the type suffix will be used to determine the returned
        /// <see cref="ClassMapping"/>, otherwise the <see cref="PropertyMapping.ImplementingType"/> is used. As well,
        /// since the property name is from the serialized form it may also be prefixed by '_'.
        /// </remarks>
        public static (PropertyMapping propMapping, ClassMapping propValueMapping) GetMappedElementMetadata(ModelInspector inspector, ClassMapping parentMapping, string propertyName)
        {
            bool startsWithUnderscore = propertyName[0] == '_';
            var elementName = startsWithUnderscore ? propertyName.Substring(1) : propertyName;

            var propertyMapping = parentMapping.FindMappedElementByName(elementName)
                ?? parentMapping.FindMappedElementByChoiceName(propertyName)
                ?? throw new JsonException($"Encountered unrecognized property '{propertyName}.'");

            ClassMapping propertyValueMapping = propertyMapping.Choice switch
            {
                ChoiceType.None or ChoiceType.ResourceChoice => SerializationUtilities.FindClassMapping(inspector, propertyMapping.ImplementingType) ??
                        throw new InvalidOperationException($"Encountered property type {propertyMapping.ImplementingType} for which no mapping was found in the model assemblies."),
                ChoiceType.DatatypeChoice => getChoiceClassMapping(),
                _ => throw new NotImplementedException("Unknown choice type.")
            };

            return (propertyMapping, propertyValueMapping);

            ClassMapping getChoiceClassMapping()
            {
                var typeSuffix = propertyName.Substring(propertyMapping.Name.Length);

                if (string.IsNullOrEmpty(typeSuffix))
                    throw new JsonException($"Choice element '{propertyMapping.Name}' is not suffixed with a type.");

                return inspector.FindClassMapping(typeSuffix) ??
                    throw new JsonException($"Choice element '{propertyMapping.Name}' is suffixed with an unrecognized type '{typeSuffix}'.");
            }
        }

    }

#endif
}

#nullable restore
