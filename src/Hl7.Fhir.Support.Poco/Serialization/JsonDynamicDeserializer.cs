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
using System.Reflection;
using System.Text.Json;
using ERR = Hl7.Fhir.Serialization.JsonSerializerErrors;

#nullable enable

namespace Hl7.Fhir.Serialization
{
    

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
            Assembly = assembly ?? throw new System.ArgumentNullException(nameof(assembly));
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

            if (reader.TokenType == JsonTokenType.None) reader.Read();

            //TODO: determineResourceType probably won't work with streaming inputs to Utf8JsonReader
            var resourceType = determineResourceType(ref reader);
            var resourceMapping = _inspector.FindClassMapping(resourceType) ??
                throw ERR.JSON201.With(ref reader, resourceType);

            if (resourceMapping.Factory() is Resource resource)
            {
                DeserializeObjectInto(resource, resourceMapping, ref reader, allowNull: false);
                return resource;
            }
            else
                throw ERR.JSON202.With(ref reader, resourceType);
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
            if (reader.CurrentState.Options.CommentHandling is not JsonCommentHandling.Skip and not JsonCommentHandling.Disallow)
                throw new InvalidOperationException("The reader must be set to ignore or refuse comments.");

            var targetType = target.GetType();
            var mapping = SerializationUtilities.FindClassMapping(_inspector, targetType) ??
                throw new ArgumentException($"Type '{targetType}' could not be located in model assembly '{Assembly}' and can therefore not be used for deserialization.", nameof(target));

            DeserializeObjectInto(target, mapping, ref reader, allowNull: false);
        }

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

                // Lookup the metadata for this property by its name to determine the expected type of the value
                var (propMapping, propValueMapping) = SerializationUtilities.GetMappedElementMetadata(_inspector, mapping, ref reader, currentPropertyName);

                // read past the property name into the value
                reader.Read();

                deserializePropertyValue(target, currentPropertyName, propMapping, propValueMapping, ref reader);
            }

            // read past object
            reader.Read();
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
        /// Reader will be on the first token after the property value upon return.</remarks>
        private void deserializePropertyValue(object target, string propertyName, PropertyMapping propertyMapping, ClassMapping propertyValueMapping, ref Utf8JsonReader reader)
        {
            if (propertyMapping.IsCollection && reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException($"Expected start of array since '{propertyName}' is a repeating element.");
            else if(!propertyMapping.IsCollection && reader.TokenType == JsonTokenType.StartArray)
                throw new JsonException($"Found the start of an array, but '{propertyName}' is not a repeating element.");

            if (propertyValueMapping.IsFhirPrimitive)
            {
                // There might be an existing value, since FhirPrimitives may be spread out over two properties
                var existing = propertyMapping.GetValue(target);

                if (propertyMapping.IsCollection)
                {
                    deserializeFhirPrimitiveList(existing as IList, propertyName, propertyValueMapping, ref reader, out var resultList);
                    propertyMapping.SetValue(target, resultList);
                }
                else
                {
                    deserializeFhirPrimitive(existing as PrimitiveType, propertyName, propertyValueMapping, ref reader, inArray: false, out var result);
                    propertyMapping.SetValue(target, result);
                }
            }
            else
            {
                // This is not a FHIR primitive, so we should not be dealing with these weird _name members.
                if (propertyName[0] == '_')
                    throw new JsonException($"Element '{propertyMapping.Name}' is not a FHIR primitive, so it should not use a '{propertyName}' property.");

                if (propertyMapping.IsCollection)
                {
                    // Read past start of array
                    reader.Read();

                    // Create a list of the type of this property's value.
                    var listInstance = propertyValueMapping.ListFactory();

                    // TODO: Adding this one by one is probably much slower than yielding and creating a new list
                    // using the IEnumerable constructor or maybe even using AddRange().
                    while (reader.TokenType != JsonTokenType.EndArray)
                    {
                        object? newArrayElement = null;
                        try
                        {
                            deserializeSingleValue(ref reader, propertyValueMapping, out newArrayElement);
                        }
                        catch (Exception ex) when (ex is JsonException or FormatException or AggregateException)
                        {
                            // recover from error
                        }
                        finally
                        {
                            listInstance.Add(newArrayElement);
                        }
                        
                    }

                    propertyMapping.SetValue(target, listInstance);

                    // Read past end of array
                    reader.Read();
                }
                else
                {
                    propertyMapping.SetValue(target, deserializeSingleValue(ref reader, propertyValueMapping));
                }
            }
        }

        private void deserializeFhirPrimitive(
            PrimitiveType? existingPrimitive, 
            string propertyName, 
            ClassMapping propertyValueMapping, 
            ref Utf8JsonReader reader, 
            bool inArray,
            out PrimitiveType result)
        {
            result = existingPrimitive ??
                propertyValueMapping.Factory() as PrimitiveType ??
                throw new ArgumentException($"Type of property '{propertyName}' should be a subtype of PrimitiveType.", nameof(propertyValueMapping));

            if (propertyName[0] != '_')
            {
                // No underscore, dealing with the 'value' property here.
                var primitiveValueProperty = propertyValueMapping.PrimitiveValueProperty ??
                    throw new InvalidOperationException($"All subclasses of {nameof(PrimitiveType)} should have a value property, but {propertyValueMapping.Name} has not.");

                object? valueResult = null;
                try
                {
                    deserializePrimitiveValue(ref reader, primitiveValueProperty.ImplementingType, allowNull: inArray, out valueResult);
                    //TODO: (optionally?) validate the FhirPrimitive we have just constructed.
                }
                finally
                {
                    result.ObjectValue = valueResult;
                }
            }
            else
                // Dealing with the complex part of a FHIR primitive, which is an object.
                DeserializeObjectInto(result, propertyValueMapping, ref reader, allowNull: inArray);

            return;
        }

        /// <summary>
        /// Deserializes a single object, either a resource, a FHIR primitive or a primitive value.
        /// </summary>
        private void deserializeSingleValue(ref Utf8JsonReader reader, ClassMapping propertyValueMapping, out object? result)
        {
            // Resources
            if (propertyValueMapping.IsResource)
            {
                var resource = DeserializeResource(ref reader);
                if(!propertyValueMapping.NativeType.IsAssignableFrom(resource.GetType()))
                    throw new InvalidOperationException($"Found a resource of type '{resource.GetType()}', but expected a '{propertyValueMapping.NativeType}'.");
                result = resource;
                return;
            }

            // primitive values (not FHIR primitives, real primitives, like Element.id)
            // Note: 'value' attributes for FHIR primitives are handled elsewhere, since that logic
            // needs to handle PrimitiveType.ObjectValue & dual properties.
            else if (propertyValueMapping.IsPrimitive)
            {
                deserializePrimitiveValue(ref reader, propertyValueMapping.NativeType, allowNull: false, out result);
                return;
            }

            // "normal" complex types & backbones
            else
            {
                var target = propertyValueMapping.Factory();
                DeserializeObjectInto(target, propertyValueMapping, ref reader, allowNull: false);
                result = target;
                return;
            }
        }

        private static string determineResourceType(ref Utf8JsonReader reader)
        {
            var originalReader = reader;    // copy the struct so we can "rewind"
            var atDepth = reader.CurrentDepth + 1;

            if (reader.TokenType != JsonTokenType.StartObject)
                throw ERR.JSON101.With(ref reader, reader.TokenType);

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
                            return reader.TokenType == JsonTokenType.String ? 
                                reader.GetString()! 
                                : throw ERR.JSON102.With(ref reader, reader.TokenType);
                        }
                    }
                }

                throw ERR.JSON103.With(ref reader);
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
        /// <remarks>A value without an error if the data could be parsed to the required type, and a value with an error if the
        /// value could not be parsed - in which case the value returned is the raw value coming in from the reader. When 
        /// Returning the raw value is useful for the codepaths where we want to report an error, but are still
        /// able to store the read value into the object model (most commonly <see cref="PrimitiveType.ObjectValue" />)</remarks>
        private void deserializePrimitiveValue(ref Utf8JsonReader reader, Type requiredType, bool allowNull, out object? value)
        {
            // Experiment with out params & throw
            var (val, err) = tryParsePrimitive(ref reader);
            value = val;
            if (err is not null) throw err;

            // Read past the value
            reader.Read();

            return;

            (object? value, JsonFhirException? error) tryParsePrimitive(ref Utf8JsonReader reader) =>
                reader.TokenType switch
                {
                    JsonTokenType.StartObject => (null, ERR.JSON104.With(ref reader)),
                    JsonTokenType.StartArray => (null, ERR.JSON105.With(ref reader)),
                    JsonTokenType.Null when allowNull => (null,null),
                    JsonTokenType.Null => (null, ERR.JSON109.With(ref reader)),
                    JsonTokenType.String when requiredType == typeof(string) => (reader.GetString(), null),
                    JsonTokenType.String when requiredType == typeof(byte[]) => readBase64(ref reader),
                    JsonTokenType.String when requiredType == typeof(DateTimeOffset) => readDateTimeOffset(ref reader),
                    JsonTokenType.String => (reader.GetString(), ERR.JSON110.With(ref reader, requiredType.Name, "string")),
                    JsonTokenType.Number => tryGetMatchingNumber(ref reader, requiredType),
                    JsonTokenType.True or JsonTokenType.False when requiredType == typeof(bool) => (reader.GetBoolean(), null),
                    JsonTokenType.True or JsonTokenType.False => (reader.GetBoolean(), ERR.JSON110.With(ref reader, requiredType.Name, "boolean")),
                
                    _ =>
                        // This would be an internal logic error, since our callers should have made sure we're
                        // on the primitive value after the property name (and the Utf8JsonReader would have complained about any
                        // other token that one that is a value).
                        // EK: I think 'Comment' is the only possible non-expected option here....
                        throw new InvalidOperationException($"Unexpected token type {reader.TokenType}."),
                };

            static (object? value, JsonFhirException? error) readBase64(ref Utf8JsonReader reader) =>
                reader.TryGetBytesFromBase64(out var bytesValue) ?
                    (bytesValue, null) : (reader.GetString(), ERR.JSON106.With(ref reader));

            static (object? value, JsonFhirException? error) readDateTimeOffset(ref Utf8JsonReader reader)
            {
                var contents = reader.GetString()!;

                return ElementModel.Types.DateTime.TryParse(contents, out var parsed) ?
                    (parsed.ToDateTimeOffset(TimeSpan.Zero), null) 
                    : (contents, ERR.JSON107.With(ref reader, contents, nameof(DateTimeOffset)));                
            }
        }

        /// <summary>
        /// This function tries to map from the json-format "generic" number to the kind
        /// of numeric type defined in the POCO.
        /// </summary>
        /// <returns>If the json number cannot be parsed into the kind of numeric required by the POCO property.</returns>
        private (object? value, JsonFhirException? error) tryGetMatchingNumber(ref Utf8JsonReader reader, Type requiredType)
        {
            object? value = null;
            var success = false;

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
                // We are not really expecting a number type, although we encountered
                // a json number.
                value = reader.GetDecimal();
                return (value, ERR.JSON110.With(ref reader, requiredType.Name, "number"));               
            }

            // We expected a number, we found a json number, but they don't match (e.g. precision etc)
            return success ? (value, null) : (value, ERR.JSON108.With(ref reader, value, requiredType.Name));
        }

        //private object? readPropertyValue(ref Utf8JsonReader reader)
        //{
        //    return reader.TokenType switch
        //    {
        //        JsonTokenType.False => false,
        //        JsonTokenType.Null => null,
        //        JsonTokenType.Number => reader.GetDecimal(),
        //        JsonTokenType.String => reader.GetString(),
        //        JsonTokenType.True => true,
        //        _ => throw new NotSupportedException($"Expecting a json property value, not a {reader.TokenType}.")
        //    };
        //}
    }


    internal class SerializationUtilities
    {
        public static ClassMapping? FindClassMapping(ModelInspector inspector, Type nativeType) =>
            inspector.FindClassMapping(nativeType) ?? inspector.ImportType(nativeType);

        /// <summary>
        /// Given a possibly suffixed property name (as encountered in the serialized form), lookup the
        /// mapping for the property and the mapping for the value of the property.
        /// </summary>
        /// <remarks>In case the name is a choice type, the type suffix will be used to determine the returned
        /// <see cref="ClassMapping"/>, otherwise the <see cref="PropertyMapping.ImplementingType"/> is used. As well,
        /// since the property name is from the serialized form it may also be prefixed by '_'.
        /// </remarks>
        internal static (PropertyMapping propMapping, ClassMapping propValueMapping) 
            GetMappedElementMetadata(ModelInspector inspector, ClassMapping parentMapping, ref Utf8JsonReader reader, string propertyName)
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
                ChoiceType.DatatypeChoice => getChoiceClassMapping(),
                _ => throw new NotImplementedException("Unknown choice type in property mapping.")
            };

            return (propertyMapping, propertyValueMapping);

            ClassMapping getChoiceClassMapping()
            {
                var typeSuffix = propertyName[propertyMapping.Name.Length..];

                if (string.IsNullOrEmpty(typeSuffix))
                    throw new JsonException($"Choice element '{propertyMapping.Name}' is not suffixed with a type.");

                return inspector.FindClassMapping(typeSuffix) ??
                    throw new JsonException($"Choice element '{propertyMapping.Name}' is suffixed with an unrecognized type '{typeSuffix}'.");
            }
        }

    }


}

#nullable restore
#endif