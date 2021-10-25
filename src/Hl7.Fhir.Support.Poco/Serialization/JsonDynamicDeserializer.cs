/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */


#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using ERR = Hl7.Fhir.Serialization.JsonSerializerErrors;

#nullable enable

namespace Hl7.Fhir.Serialization
{
    internal record PartialDeserialization<T>(T? PartialResult, Exception? Exception)
    {
        public PartialDeserialization<object> ToObjectResult() => new(PartialResult, Exception);
    }

    /// <summary>
    /// Deserializes a byte stream into FHIR POCO objects.
    /// </summary>
    /// <remarks>The serializer uses the format documented in https://www.hl7.org/fhir/json.html. </remarks>
    public class JsonDynamicDeserializer
    {
        /// <summary>
        /// Initializes an instante of the deserializer.
        /// </summary>
        /// <param name="assembly">Assembly containing the POCO classes to be used for deserialization.</param>
        public JsonDynamicDeserializer(Assembly assembly)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            _inspector = ModelInspector.ForAssembly(assembly);
        }

        /// <summary>
        /// Assembly containing the POCO classes the deserializer will use to deserialize data into.
        /// </summary>
        public Assembly Assembly { get; }

        private readonly ModelInspector _inspector;

        /// <summary>
        /// Deserialize the FHIR Json from the reader and create a new POCO object containing the data from the reader.
        /// </summary>
        public Resource DeserializeResource(ref Utf8JsonReader reader)
        {
            if (reader.CurrentState.Options.CommentHandling is not JsonCommentHandling.Skip and not JsonCommentHandling.Disallow)
                throw new InvalidOperationException("The reader must be set to ignore or refuse comments.");

            // If the stream has just been opened, move to the first token.
            if (reader.TokenType == JsonTokenType.None) reader.Read();

            var (result, exception) = DeserializeResourceInternal(ref reader);
            
            return exception is null ? result! : throw DeserializationFailedException.Create(result, exception);
        }

        internal PartialDeserialization<Resource> DeserializeResourceInternal(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                return new(null,ERR.JSON101.With(ref reader, reader.TokenType));

            try
            {
                //TODO: determineResourceType probably won't work with streaming inputs to Utf8JsonReader                       
                var outcome = determineResourceType(ref reader);
                if(outcome.Exception is not null) throw outcome.Exception;

                var resourceType = outcome.PartialResult!;
                var resourceMapping = _inspector.FindClassMapping(resourceType) ??
                    throw ERR.JSON201.With(ref reader, resourceType);

                if (resourceMapping.Factory() is Resource newResource)
                    return DeserializeObjectIntoInternal(newResource, resourceMapping, ref reader, allowNull: false);
                else
                    throw ERR.JSON202.With(ref reader, resourceType);
            }
            catch (JsonFhirException jfe)
            {
                // recover by reading until *past* the end of the object
                do { reader.Read(); } while(reader.TokenType != JsonTokenType.EndObject);
                reader.Read();

                return new(null, jfe);
            }
        }

        public T DeserializeObject<T>(ref Utf8JsonReader reader) where T : Base => (T)DeserializeObject(typeof(T), ref reader);

        public Base DeserializeObject(Type targetType, ref Utf8JsonReader reader)
        {
            // If the stream has just been opened, move to the first token.
            if (reader.TokenType == JsonTokenType.None) reader.Read();

            var mapping = FindClassMapping(_inspector, targetType) ??
                throw new ArgumentException($"Type '{targetType}' could not be located in model assembly '{Assembly}' and can therefore not be used for deserialization.", nameof(targetType));

            // Create a new instance of the object to read the members into.
            if (mapping.Factory() is Base b)
            {
                DeserializeObjectIntoInternal(b, mapping, ref reader, allowNull: false);
                return b;
            }
            else
                throw new ArgumentException($"Can only deserialize into subclasses of class {nameof(Base)}.", nameof(targetType));
        }

        internal PartialDeserialization<T> DeserializeObjectIntoInternal<T>(T target, ClassMapping mapping, ref Utf8JsonReader reader, bool allowNull)
            where T : Base
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                var err = allowNull ? null : ERR.JSON109.With(ref reader);
                reader.Read();
                return new(target, err);
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                // TODO: we should try some recovery here. It can only be a single token (which we can
                // just Read(), or an array - and we should just skip the array
                return new(target, ERR.JSON101.With(ref reader, reader.TokenType));
            }

            // read past start of object into first property or end of object
            reader.Read();

            ExceptionAggregator aggregator = new();

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                var currentPropertyName = reader.GetString()!;

                //TODO: if this is not a resource, there should not be a resource type.
                if (currentPropertyName == "resourceType")
                {
                    reader.Read(); // into value
                    reader.Read(); // into next
                    continue;
                }

                try
                {
                    // Lookup the metadata for this property by its name to determine the expected type of the value
                    var (propMapping, propValueMapping) = getMappedElementMetadata(_inspector, mapping, ref reader, currentPropertyName);

                    // read past the property name into the value
                    reader.Read();

                    var partial = deserializePropertyValue(target, currentPropertyName, propMapping, propValueMapping, ref reader);
                    aggregator.Add(partial.Exception);
                }
                catch (JsonFhirException jfe)
                {
                    aggregator.Add(jfe);

                    // try to recover by skipping this property - next propertyname or endobject AT THE SAME DEPTH!!!!!
                    do
                    {
                        reader.Read();
                    }
                    //TODO:at the same depth
                    while (reader.TokenType != JsonTokenType.EndObject && reader.TokenType != JsonTokenType.PropertyName);
                }
            }

            // read past object
            reader.Read();

            return new(target, aggregator.Aggregate());
        }

        /// <summary>
        /// Reads the value of a json property. 
        /// </summary>
        /// <param name="target">The target POCO which property will be set/updated during deserialization. If null, it will be
        /// be created based on the <paramref name="propertyMapping"/>, otherwise it will be updated.</param>
        /// <param name="propertyName">The literal name of the property in the json serialization.</param>
        /// <param name="propertyMapping">The cached metadata for the POCO's property we are setting.</param>
        /// <param name="propertyValueMapping">The cached metadata for the type of the property to set.</param>
        /// <param name="reader">The reader to deserialize from.</param>
        /// <remarks>Expects the reader to be positioned on the property value.
        /// Reader will be on the first token after the property value upon return. In case of an error, the reader will be on
        /// the position of the error and the caller needs to implement recovery.</remarks>
        private PartialDeserialization<Base> deserializePropertyValue(Base target, string propertyName, PropertyMapping propertyMapping, ClassMapping propertyValueMapping, ref Utf8JsonReader reader)
        {
            // TODO: This needs recovery (unless the one caller already does that - I believe that is the case)
            if (propertyMapping.IsCollection && reader.TokenType != JsonTokenType.StartArray)
                return new(target, ERR.JSON111.With(ref reader, propertyName));
            else if (!propertyMapping.IsCollection && reader.TokenType == JsonTokenType.StartArray)
                return new(target, ERR.JSON112.With(ref reader, propertyName));

            if (propertyValueMapping.IsFhirPrimitive)
            {
                // There might be an existing value, since FhirPrimitives may be spread out over two properties
                // (one with, and one without the '_')
                var existingValue = propertyMapping.GetValue(target);

                if (propertyMapping.IsCollection)
                {
                    var result = deserializeFhirPrimitiveList(existingValue as IList, propertyName, propertyValueMapping, ref reader);
                    propertyMapping.SetValue(target, result.PartialResult);
                    return new(target, result.Exception);
                }
                else
                {
                    var result = deserializeFhirPrimitive(existingValue as PrimitiveType, propertyName, propertyValueMapping, ref reader, inArray: false);
                    propertyMapping.SetValue(target, result);
                    return new(target, result.Exception);
                }
            }
            else
            {
                // This is not a FHIR primitive, so we should not be dealing with these weird _name members.
                if (propertyName[0] == '_') return new(target, ERR.JSON113.With(ref reader, propertyMapping.Name, propertyName));

                if (propertyMapping.IsCollection)
                {
                    // Read past start of array
                    reader.Read();

                    // Create a list of the type of this property's value.
                    var listInstance = propertyValueMapping.ListFactory();

                    var aggregator = new ExceptionAggregator();

                    // TODO: Adding this one by one is probably much slower than yielding and creating a new list
                    // using the IEnumerable constructor or maybe even using AddRange().
                    while (reader.TokenType != JsonTokenType.EndArray)
                    {
                        var result = deserializeSingleValue(ref reader, propertyValueMapping);
                        listInstance.Add(result.PartialResult);
                        aggregator.Add(result.Exception);
                    }

                    propertyMapping.SetValue(target, listInstance);

                    // Read past end of array
                    reader.Read();

                    return new(target, aggregator.Aggregate());
                }
                else
                {
                    var result = deserializeSingleValue(ref reader, propertyValueMapping);
                    propertyMapping.SetValue(target, result.PartialResult);

                    return new(target, result.Exception);
                }
            }
        }

        private PartialDeserialization<PrimitiveType> deserializeFhirPrimitive(
            PrimitiveType? existingPrimitive,
            string propertyName,
            ClassMapping propertyValueMapping,
            ref Utf8JsonReader reader,
            bool inArray)
        {
            var targetPrimitive = existingPrimitive ?? (PrimitiveType)propertyValueMapping.Factory();

            if (propertyName[0] != '_')
            {
                // No underscore, dealing with the 'value' property here.
                var primitiveValueProperty = propertyValueMapping.PrimitiveValueProperty ??
                    throw new InvalidOperationException($"All subclasses of {nameof(PrimitiveType)} should have a value property, but {propertyValueMapping.Name} has not.");

                var result = DeserializePrimitiveValue(ref reader, primitiveValueProperty.ImplementingType, allowNull: inArray);
                targetPrimitive.ObjectValue = result.PartialResult;
                //TODO: validate the targetPrimitive?

                return new(targetPrimitive, result.Exception);
            }
            else
            {
                // The complex part of a primitive - read the object's primitives into the target
                return DeserializeObjectIntoInternal(targetPrimitive, propertyValueMapping, ref reader, allowNull: true);
            }
        }

        private PartialDeserialization<IList> deserializeFhirPrimitiveList(IList? existingPrimitiveList, string propertyName, ClassMapping propertyValueMapping, ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                return new(existingPrimitiveList, ERR.JSON111.With(ref reader, propertyName));

            // read into array
            reader.Read();

            var existed = existingPrimitiveList is not null;
            IList resultList = existingPrimitiveList ??
                propertyValueMapping.ListFactory() ??
                throw new ArgumentException($"Type of property '{propertyName}' should be a subtype of IList<PrimitiveType>.", nameof(propertyValueMapping));

            // TODO: We can speed this up by having a codepath for adding to existing items,
            // and having a fresh (yield based) factory returning an IEnumerable and then initializing
            // a new list with this IEnumerable.
            int elementIndex = 0;
            ExceptionAggregator aggregator = new();

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (elementIndex >= resultList.Count)
                {
                    //TODO: not an empty array
                    //TODO: not an array with just nulls
                    //TODO: x and _x may not a null at the same index
                    //TODO: x and _x should have the same # of items
                    resultList.Add((PrimitiveType)propertyValueMapping.Factory());
                }

                var targetPrimitive = (PrimitiveType)resultList[elementIndex]!;

                var result = deserializeFhirPrimitive(targetPrimitive, propertyName, propertyValueMapping, ref reader, inArray: true);
                aggregator.Add(result.Exception);

                elementIndex += 1;
            }

            // read past array to next property or end of object
            reader.Read();

            return new(resultList, aggregator.Aggregate());
        }

        /// <summary>
        /// Deserializes a single object, either a resource, a FHIR primitive or a primitive value.
        /// </summary>
        private PartialDeserialization<object> deserializeSingleValue(ref Utf8JsonReader reader, ClassMapping propertyValueMapping)
        {
            // Resources
            if (propertyValueMapping.IsResource)
            {
                var result = DeserializeResourceInternal(ref reader);
                if (!propertyValueMapping.NativeType.IsAssignableFrom(result.GetType()))
                    throw new InvalidOperationException($"Found a resource of type '{result.GetType()}', but expected a '{propertyValueMapping.NativeType}'.");

                return result.ToObjectResult();
            }

            // primitive values (not FHIR primitives, real primitives, like Element.id)
            // Note: 'value' attributes for FHIR primitives are handled elsewhere, since that logic
            // needs to handle PrimitiveType.ObjectValue & dual properties.
            else if (propertyValueMapping.IsPrimitive)
            {
                return DeserializePrimitiveValue(ref reader, propertyValueMapping.NativeType, allowNull: false);
            }

            // "normal" complex types & backbones
            else
            {
                var newComplex = (Base)propertyValueMapping.Factory();
                return DeserializeObjectIntoInternal(newComplex, propertyValueMapping, ref reader, allowNull: false).ToObjectResult();
            }
        }

        private static void SkipTo(ref Utf8JsonReader reader, JsonTokenType tt)
        {
            var depth = reader.CurrentDepth;

            while (reader.Read() && !(reader.CurrentDepth == depth && reader.TokenType == tt)) ;
        }

        private static PartialDeserialization<string> determineResourceType(ref Utf8JsonReader reader)
        {
            var originalReader = reader;    // copy the struct so we can "rewind"
            var atDepth = reader.CurrentDepth + 1;

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
                            return (reader.TokenType == JsonTokenType.String) ?
                                new(reader.GetString()!, null) :
                                new(null, ERR.JSON102.With(ref reader, reader.TokenType));
                        }
                    }
                }

                return new(null, ERR.JSON103.With(ref reader));
            }
            finally
            {
                reader = originalReader;
            }
        }


        /// <summary>
        /// Does a best-effort parse of the data available at the reader, given the required type of the property the
        /// data needs to be read into. 
        /// </summary>
        /// <returns>A value without an error if the data could be parsed to the required type, and a value with an error if the
        /// value could not be parsed - in which case the value returned is the raw value coming in from the reader.</returns>
        /// <remarks>Upon return, the reader will be positioned after the primitive. Returning the raw value is useful for the
        /// codepaths where we want to report an error, but are still
        /// able to store the read value into the object model (most commonly <see cref="PrimitiveType.ObjectValue" />)</remarks>
        internal static PartialDeserialization<object> DeserializePrimitiveValue(ref Utf8JsonReader reader, Type requiredType, bool allowNull)
        {
            var result = reader.TokenType switch
            {
                JsonTokenType.StartObject => recover(ref reader, null, ERR.JSON104.With(ref reader), JsonTokenType.EndObject),
                JsonTokenType.StartArray => recover(ref reader, null, ERR.JSON105.With(ref reader), JsonTokenType.EndArray),
                JsonTokenType.Null when allowNull => new(null, null),
                JsonTokenType.Null => new(null, ERR.JSON109.With(ref reader)),
                JsonTokenType.String when requiredType == typeof(string) => new(reader.GetString(), null),
                JsonTokenType.String when requiredType == typeof(byte[]) => readBase64(ref reader),
                JsonTokenType.String when requiredType == typeof(DateTimeOffset) => readDateTimeOffset(ref reader),
                JsonTokenType.String when requiredType.IsEnum => new(reader.GetString(), null),
                JsonTokenType.String => new(reader.GetString(), ERR.JSON110.With(ref reader, requiredType.Name, "string")),
                JsonTokenType.Number => tryGetMatchingNumber(ref reader, requiredType),
                JsonTokenType.True or JsonTokenType.False when requiredType == typeof(bool) => new(reader.GetBoolean(), null),
                JsonTokenType.True or JsonTokenType.False => new(reader.GetBoolean(), ERR.JSON110.With(ref reader, requiredType.Name, "boolean")),

                _ => 
                    // This would be an internal logic error, since our callers should have made sure we're
                    // on the primitive value after the property name (and the Utf8JsonReader would have complained about any
                    // other token that one that is a value).
                    // EK: I think 'Comment' is the only possible non-expected option here....
                    throw new InvalidOperationException($"Unexpected token type {reader.TokenType} while parsing a primitive value. " +
                        ERR.GenerateLocationMessage(ref reader, out var _, out var _)),
            };

            // Read past the value
            reader.Read();

            return result;

            static PartialDeserialization<object> recover(ref Utf8JsonReader reader, object? result, JsonFhirException error, JsonTokenType recoveryToken)
            {
                SkipTo(ref reader, recoveryToken);
                return new(result, error);
            }

            static PartialDeserialization<object> readBase64(ref Utf8JsonReader reader) =>
                reader.TryGetBytesFromBase64(out var bytesValue) ?
                    new(bytesValue, null) : 
                    new(reader.GetString(), ERR.JSON106.With(ref reader));

            static PartialDeserialization<object> readDateTimeOffset(ref Utf8JsonReader reader)
            {
                var contents = reader.GetString()!;

                return ElementModel.Types.DateTime.TryParse(contents, out var parsed) ?
                    new (parsed.ToDateTimeOffset(TimeSpan.Zero), null) :
                    new (contents, ERR.JSON107.With(ref reader, contents));
            }
        }

        /// <summary>
        /// This function tries to map from the json-format "generic" number to the kind of numeric type defined in the POCO.
        /// </summary>
        /// <remarks>Reader must be positioned on a number token. This function will not move the reader to the next token.</remarks>
        private static PartialDeserialization<object> tryGetMatchingNumber(ref Utf8JsonReader reader, Type requiredType)
        {
            if (reader.TokenType != JsonTokenType.Number) throw new InvalidOperationException($"Cannot read a numeric when reader is on a {reader.TokenType}.");

            object? value = null;
            bool success;

            if (requiredType == typeof(decimal))
                success = reader.TryGetDecimal(out decimal dec) && (value = dec) is { };
            else if (requiredType == typeof(int))
                success = reader.TryGetInt32(out int i32) && (value = i32) is { };
            else if (requiredType == typeof(uint))
                success = reader.TryGetUInt32(out uint ui32) && (value = ui32) is { };
            else if (requiredType == typeof(long))
                success = reader.TryGetInt64(out long i64) && (value = i64) is { };
            else if (requiredType == typeof(ulong))
                success = reader.TryGetUInt64(out ulong ui64) && (value = ui64) is { };
            else if (requiredType == typeof(float))
                success = reader.TryGetSingle(out float si) && (value = si) is { };
            else if (requiredType == typeof(double))
                success = reader.TryGetDouble(out double dbl) && (value = dbl) is { };
            else
            {
                var gotValue = reader.TryGetDecimal(out var dec);
                return new(gotValue ? dec : null, ERR.JSON110.With(ref reader, requiredType.Name, "number"));
            }

            // We expected a number, we found a json number, but they don't match (e.g. precision etc)
            if(success)
            {
                return new(value, null);
            }
            else
            {
                var gotValue = reader.TryGetDecimal(out var dec);
                return new(gotValue ? dec : null, ERR.JSON108.With(ref reader, gotValue ? dec : "(unreadable number)", requiredType.Name));
            }
        }

        internal static ClassMapping? FindClassMapping(ModelInspector inspector, Type nativeType) =>
            inspector.FindClassMapping(nativeType) ?? inspector.ImportType(nativeType);

        /// <summary>
        /// Given a possibly suffixed property name (as encountered in the serialized form), lookup the
        /// mapping for the property and the mapping for the value of the property.
        /// </summary>
        /// <remarks>In case the name is a choice type, the type suffix will be used to determine the returned
        /// <see cref="ClassMapping"/>, otherwise the <see cref="PropertyMapping.ImplementingType"/> is used. As well,
        /// since the property name is from the serialized form it may also be prefixed by '_'.
        /// </remarks>
        private static (PropertyMapping propMapping, ClassMapping propValueMapping)
            getMappedElementMetadata(ModelInspector inspector, ClassMapping parentMapping, ref Utf8JsonReader reader, string propertyName)
        {
            bool startsWithUnderscore = propertyName[0] == '_';
            var elementName = startsWithUnderscore ? propertyName[1..] : propertyName;

            var propertyMapping = parentMapping.FindMappedElementByName(elementName)
                ?? parentMapping.FindMappedElementByChoiceName(propertyName)
                ?? throw ERR.JSON203.With(ref reader, propertyName);

            ClassMapping propertyValueMapping = propertyMapping.Choice switch
            {
                ChoiceType.None or ChoiceType.ResourceChoice => FindClassMapping(inspector, propertyMapping.ImplementingType) ??
                        throw new InvalidOperationException($"Encountered property type {propertyMapping.ImplementingType} for which no mapping was found in the model assemblies."),
                ChoiceType.DatatypeChoice => getChoiceClassMapping(ref reader),
                _ => throw new NotImplementedException("Unknown choice type in property mapping.")
            };

            return (propertyMapping, propertyValueMapping);

            ClassMapping getChoiceClassMapping(ref Utf8JsonReader r)
            {
                string typeSuffix = propertyName[propertyMapping.Name.Length..];

                if (string.IsNullOrEmpty(typeSuffix))
                    throw ERR.JSON114.With(ref r, propertyMapping.Name);

                return inspector.FindClassMapping(typeSuffix) ??
                    throw ERR.JSON115.With(ref r, propertyMapping.Name, typeSuffix);
            }
        }

    }


}

#nullable restore
#endif