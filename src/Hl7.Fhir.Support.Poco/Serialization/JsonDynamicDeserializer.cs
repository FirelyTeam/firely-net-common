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
        /// <param name="reader">A json reader positioned on the first token of the object, or the beginning of the stream.</param>
        /// <returns>A fully initialized POCO with the data from the reader.</returns>
        public Resource DeserializeResource(ref Utf8JsonReader reader)
        {
            if (reader.CurrentState.Options.CommentHandling is not JsonCommentHandling.Skip and not JsonCommentHandling.Disallow)
                throw new InvalidOperationException("The reader must be set to ignore or refuse comments.");

            // If the stream has just been opened, move to the first token.
            if (reader.TokenType == JsonTokenType.None) reader.Read();

            var (result, exception) = DeserializeResourceInternal(ref reader);
            
            return exception is null ? (Resource)result! : throw DeserializationFailedException.Create(result, exception);
        }

        /// <summary>
        /// Reads a (subtree) of serialzed FHIR Json data into a POCO object.
        /// </summary>
        /// <param name="targetType">The type of POCO to construct and deserialize</param>
        /// <param name="reader">A json reader positioned on the first token of the object, or the beginning of the stream.</param>
        /// <returns>A fully initialized POCO with the data from the reader.</returns>
        public Base DeserializeObject(Type targetType, ref Utf8JsonReader reader)
        {
            if (reader.CurrentState.Options.CommentHandling is not JsonCommentHandling.Skip and not JsonCommentHandling.Disallow)
                throw new InvalidOperationException("The reader must be set to ignore or refuse comments.");

            // If the stream has just been opened, move to the first token.
            if (reader.TokenType == JsonTokenType.None) reader.Read();

            var mapping = FindClassMapping(_inspector, targetType) ??
                throw new ArgumentException($"Type '{targetType}' could not be located in model assembly '{Assembly}' and can " +
                    $"therefore not be used for deserialization. " + ERR.GenerateLocationMessage(ref reader), nameof(targetType));

            // Create a new instance of the object to read the members into.
            if (mapping.Factory() is Base b)
            {
                var (result, exception) = DeserializeObjectInto(b, mapping, ref reader, inResource: false);
                return exception is null ? result! : throw DeserializationFailedException.Create(result, exception);
            }
            else
                throw new ArgumentException($"Can only deserialize into subclasses of class {nameof(Base)}. " + ERR.GenerateLocationMessage(ref reader), nameof(targetType));
        }

        /// <summary>
        /// Reads a (subtree) of serialzed FHIR Json data into a POCO object.
        /// </summary>
        /// <typeparam name="T">The type of POCO to construct and deserialize</typeparam>
        /// <param name="reader">A json reader positioned on the first token of the object, or the beginning of the stream.</param>
        /// <returns>A fully initialized POCO with the data from the reader.</returns>
        public T DeserializeObject<T>(ref Utf8JsonReader reader) where T : Base => (T)DeserializeObject(typeof(T), ref reader);

        internal PartialDeserialization<Base> DeserializeResourceInternal(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                reader.Recover();  // skip to the end of the construct encountered (value or array)
                return new(null, ERR.EXPECTED_START_OF_OBJECT.With(ref reader, reader.TokenType));
            }

            var outcome = DetermineClassMappingFromInstance(ref reader, _inspector);
            if (outcome.Success)
            {
                // If we have at least a mapping, let's try to continue               
                var resourceMapping = outcome.PartialResult!;                
                var newResource = (Base)resourceMapping.Factory();
                var result = DeserializeObjectInto(newResource, resourceMapping, ref reader, inResource: true);

                ExceptionAggregator aggregator = new();
                if (!resourceMapping.IsResource)
                    aggregator.Add(ERR.EXPECTED_A_RESOURCE_TYPE.With(ref reader, resourceMapping.Name));

                aggregator.Add(result.Exception);

                return new(result.PartialResult, aggregator.Aggregate());
            }
            else
            {
                // Read past the end of this object to recover.
                reader.Recover();
                return new(null, outcome.Exception);
            }
        }

        /// <summary>
        /// Reads a complex object into an existing instance of a POCO.
        /// </summary>
        /// <remarks>Reader will be on the first token after the object upon return.</remarks>
        internal PartialDeserialization<T> DeserializeObjectInto<T>(T target, ClassMapping mapping, ref Utf8JsonReader reader, bool inResource)
            where T : Base
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                reader.Recover();  // skip to the end of the construct encountered (value or array)
                return new(target, ERR.EXPECTED_START_OF_OBJECT.With(ref reader, reader.TokenType));
            }

            // read past start of object into first property or end of object
            reader.Read();

            ExceptionAggregator aggregator = new();
            var empty = true;
            var plps = new FhirPrimitiveListParsingState();

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                var currentPropertyName = reader.GetString()!;

                if (currentPropertyName == "resourceType")
                {
                    if (!inResource) aggregator.Add(ERR.RESOURCETYPE_UNEXPECTED_IN_DT.With(ref reader));
                    reader.SkipTo(JsonTokenType.PropertyName);  // skip to next property
                    continue;
                }

                empty = false;
                PropertyMapping? propMapping;
                ClassMapping? propValueMapping;

                try
                {
                    // Lookup the metadata for this property by its name to determine the expected type of the value
                    (propMapping, propValueMapping) = getMappedElementMetadata(_inspector, mapping, ref reader, currentPropertyName);
                }
                catch (JsonFhirException jfe)
                {
                    aggregator.Add(jfe);

                    // try to recover by skipping to the next property.
                    reader.SkipTo(JsonTokenType.PropertyName);
                    continue;
                }

                // read past the property name into the value
                reader.Read();

                var partial = deserializePropertyValue(target, currentPropertyName, propMapping, propValueMapping, ref reader, plps);
                aggregator.Add(partial.Exception);

            }

            // check for single array properties containing a null
            aggregator.Add(plps.Check(ref reader));

            // read past object
            reader.Read();

            // do not allow empty complex objects.
            if (empty) aggregator.Add(ERR.OBJECTS_CANNOT_BE_EMPTY.With(ref reader));

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
        /// <param name="plps">State used internally by the parser.</param>
        /// <remarks>Expects the reader to be positioned on the property value.
        /// Reader will be on the first token after the property value upon return.</remarks>
        private PartialDeserialization<Base> deserializePropertyValue(
            Base target, 
            string propertyName, 
            PropertyMapping propertyMapping, 
            ClassMapping propertyValueMapping, 
            ref Utf8JsonReader reader,
            FhirPrimitiveListParsingState plps
            )
        {
            if (propertyValueMapping.IsFhirPrimitive)
            {
                // There might be an existing value, since FhirPrimitives may be spread out over two properties
                // (one with, and one without the '_')
                var existingValue = propertyMapping.GetValue(target);

                if (propertyMapping.IsCollection)
                {
                    // Note that the POCO model will always allocate a new list if the property had not been set before,
                    // so there is always an existingValue;
                    var result = deserializeFhirPrimitiveList((IList)existingValue!, propertyName, propertyValueMapping, ref reader, plps);
                    propertyMapping.SetValue(target, result.PartialResult);
                    return new(target, result.Exception);
                }
                else
                {
                    var result = DeserializeFhirPrimitive(existingValue as PrimitiveType, propertyName, propertyValueMapping, ref reader);
                    propertyMapping.SetValue(target, result.PartialResult);
                    return new(target, result.Exception);
                }
            }
            else
            {
                ExceptionAggregator aggregator = new();

                // This is not a FHIR primitive, so we should not be dealing with these weird _name members.
                if (propertyName[0] == '_') 
                    aggregator.Add(ERR.USE_OF_UNDERSCORE_ILLEGAL.With(ref reader, propertyMapping.Name, propertyName));

                PartialDeserialization<object> result = propertyMapping.IsCollection
                    ? deserializeNormalList(propertyName, propertyValueMapping, ref reader).Cast<object>()
                    : deserializeSingleValue(ref reader, propertyValueMapping);

                aggregator.Add(result.Exception);
                propertyMapping.SetValue(target, result.PartialResult);
                return new(target, aggregator.Aggregate());
            }
        }

        /// <summary>
        /// Reads the content of a list with non-FHIR-primitive content (so, no name/_name pairs to be dealt with). Note
        /// that the contents can only be complex in the current FHIR serialization, but we'll be prepared and handle
        /// other situations (e.g. repeating Extension.url's, if they would ever exist).
        /// </summary>
        private PartialDeserialization<IList> deserializeNormalList(string propertyName, ClassMapping propertyValueMapping, ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                reader.Recover();        // skip property value completely
                return new(null, ERR.EXPECTED_START_OF_ARRAY.With(ref reader, propertyName));
            }

            // Read past start of array
            reader.Read();

            // Create a list of the type of this property's value.
            var listInstance = propertyValueMapping.ListFactory();
            var aggregator = new ExceptionAggregator();

            if (reader.TokenType == JsonTokenType.EndArray)
                aggregator.Add(ERR.ARRAYS_CANNOT_BE_EMPTY.With(ref reader));

            // Can't make an iterator because of the ref readers struct, so need
            // to simply create a list by Adding(). Not the fastest approach :-(
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                var result = deserializeSingleValue(ref reader, propertyValueMapping);
                listInstance.Add(result.PartialResult);
                aggregator.Add(result.Exception);
            }

            // Read past end of array
            reader.Read();

            return new(listInstance, aggregator.Aggregate());
        }

        private class FhirPrimitiveListParsingState
        {
            public Dictionary<string, string?> SingleArraysWithNull = new();

            public JsonFhirException? Check(ref Utf8JsonReader reader)
            {
                var relevantElements = SingleArraysWithNull.Where(kvp => kvp.Value is not null).Select(kvp => kvp.Key);

                if (relevantElements.Any())
                {
                    return ERR.PRIMITIVE_ARRAYS_LONELY_NULL.With(ref reader, string.Join(", ", relevantElements));
                }
                else
                    return null;
            }
             
        }

        /// <summary>
        /// Reads a list of FHIR primitives (either from a name or _name property).
        /// </summary>
        /// <remarks>Upon completion, reader will be located at the next token afther the list.</remarks>
        private PartialDeserialization<IList> deserializeFhirPrimitiveList(
            IList existingList, 
            string propertyName, 
            ClassMapping propertyValueMapping, 
            ref Utf8JsonReader reader,
            FhirPrimitiveListParsingState state
            )
        {
            bool hadLonely = 
                state.SingleArraysWithNull.Any() &&
                state.SingleArraysWithNull.Remove(propertyName.TrimStart('_')); 

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                reader.Recover();        // skip the whole array
                return new(null, ERR.EXPECTED_START_OF_ARRAY.With(ref reader, propertyName));
            }

            // read into array
            reader.Read();

            int originalSize = existingList.Count;

            // Can't make an iterator because of the ref readers struct, so need
            // to simply create a list by Adding(). Not the fastest approach :-(
            int elementIndex = 0;
            ExceptionAggregator aggregator = new();

            if (reader.TokenType == JsonTokenType.EndArray)
            {
                aggregator.Add(ERR.ARRAYS_CANNOT_BE_EMPTY.With(ref reader));
                if(!hadLonely) state.SingleArraysWithNull.Add(propertyName.TrimStart('_'), null);
            }

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (elementIndex >= originalSize)
                    existingList.Add(propertyValueMapping.Factory());

                var targetPrimitive = (PrimitiveType)existingList[elementIndex]!;

                if(reader.TokenType == JsonTokenType.Null)
                {
                    if (originalSize == 0 && !hadLonely) state.SingleArraysWithNull.TryAdd(propertyName.TrimStart('_'), propertyName);

                    if (originalSize > 0 && elementIndex < originalSize && targetPrimitive.Count() == 0)
                        aggregator.Add(ERR.PRIMITIVE_ARRAYS_BOTH_NULL.With(ref reader));

                    // don't read aby any new data into the primitive here
                    reader.Read();
                }
                else
                {
                    var result = DeserializeFhirPrimitive(targetPrimitive, propertyName, propertyValueMapping, ref reader);
                    aggregator.Add(result.Exception);
                }

                elementIndex += 1;
            }

            if (originalSize > 0 && elementIndex != originalSize)
                aggregator.Add(ERR.PRIMITIVE_ARRAYS_INCOMPAT_SIZE.With(ref reader));

            // read past array to next property or end of object
            reader.Read();

            return new(existingList, aggregator.Aggregate());
        }

        /// <summary>
        /// Deserializes a FHIR primitive, which can be a name or _name property.
        /// </summary>
        /// <remarks>Upon completion, reader will be located at the next token afther the FHIR primitive.</remarks>
        internal PartialDeserialization<PrimitiveType> DeserializeFhirPrimitive(
            PrimitiveType? existingPrimitive,
            string propertyName,
            ClassMapping propertyValueMapping,
            ref Utf8JsonReader reader)
        {
            var targetPrimitive = existingPrimitive ?? (PrimitiveType)propertyValueMapping.Factory();

            if (propertyName[0] != '_')
            {
                // No underscore, dealing with the 'value' property here.
                var primitiveValueProperty = propertyValueMapping.PrimitiveValueProperty ??
                    throw new InvalidOperationException($"All subclasses of {nameof(PrimitiveType)} should have a value property, " +
                        $"but {propertyValueMapping.Name} has not. " + ERR.GenerateLocationMessage(ref reader));

                var result = DeserializePrimitiveValue(ref reader, primitiveValueProperty.ImplementingType);
                targetPrimitive.ObjectValue = result.PartialResult;
                //TODO: validate the targetPrimitive?

                return new(targetPrimitive, result.Exception);
            }
            else
            {
                // The complex part of a primitive - read the object's primitives into the target
                return DeserializeObjectInto(targetPrimitive, propertyValueMapping, ref reader, inResource:false);
            }
        }

        /// <summary>
        /// Deserializes a single object, either a resource, a FHIR primitive or a primitive value.
        /// </summary>
        /// <remarks>Upon completion, reader will be located at the next token afther the value.</remarks>
        private PartialDeserialization<object> deserializeSingleValue(ref Utf8JsonReader reader, ClassMapping propertyValueMapping)
        {
            // Resources
            if (propertyValueMapping.IsResource)
            {
                var result = DeserializeResourceInternal(ref reader);
                return (result.PartialResult is null || propertyValueMapping.NativeType.IsAssignableFrom(result.PartialResult.GetType()))
                    ? result.Cast<object>()
                    : throw new InvalidOperationException($"Found a resource of type '{result.GetType()}', but expected a " +
                        $"'{propertyValueMapping.NativeType}'. " + ERR.GenerateLocationMessage(ref reader));
            }

            // primitive values (not FHIR primitives, real primitives, like Element.id)
            // Note: 'value' attributes for FHIR primitives are handled elsewhere, since that logic
            // needs to handle PrimitiveType.ObjectValue & dual properties.
            else if (propertyValueMapping.IsPrimitive)
            {
                return DeserializePrimitiveValue(ref reader, propertyValueMapping.NativeType);
            }

            // "normal" complex types & backbones
            else
            {
                var newComplex = (Base)propertyValueMapping.Factory();
                return DeserializeObjectInto(newComplex, propertyValueMapping, ref reader, inResource:false).Cast<object>();
            }
        }        

        /// <summary>
        /// Does a best-effort parse of the data available at the reader, given the required type of the property the
        /// data needs to be read into. 
        /// </summary>
        /// <returns>A value without an error if the data could be parsed to the required type, and a value with an error if the
        /// value could not be parsed - in which case the value returned is the raw value coming in from the reader.</returns>
        /// <remarks>Upon completion, the reader will be positioned on the token after the primitive.</remarks>
        internal static PartialDeserialization<object> DeserializePrimitiveValue(ref Utf8JsonReader reader, Type requiredType)
        {
            // Check for unexpected non-value types.
            if (reader.TokenType == JsonTokenType.StartObject) return recover(ref reader, null, ERR.EXPECTED_PRIMITIVE_NOT_OBJECT.With(ref reader));
            if (reader.TokenType == JsonTokenType.StartArray) return recover(ref reader, null, ERR.EXPECTED_PRIMITIVE_NOT_ARRAY.With(ref reader));

            // Check for value types
            var result = reader.TokenType switch
            {
                JsonTokenType.Null => new(null, ERR.EXPECTED_PRIMITIVE_NOT_NULL.With(ref reader)),
                JsonTokenType.String when requiredType == typeof(string) => new(reader.GetString(), null),
                JsonTokenType.String when requiredType == typeof(byte[]) => readBase64(ref reader),
                JsonTokenType.String when requiredType == typeof(DateTimeOffset) => readDateTimeOffset(ref reader),
                JsonTokenType.String when requiredType.IsEnum => new(reader.GetString(), null),
                JsonTokenType.String => new(reader.GetString(), ERR.UNEXPECTED_JSON_TOKEN.With(ref reader, requiredType.Name, "string")),
                JsonTokenType.Number => tryGetMatchingNumber(ref reader, requiredType),
                JsonTokenType.True or JsonTokenType.False when requiredType == typeof(bool) => new(reader.GetBoolean(), null),
                JsonTokenType.True or JsonTokenType.False => new(reader.GetBoolean(), ERR.UNEXPECTED_JSON_TOKEN.With(ref reader, requiredType.Name, "boolean")),

                _ => 
                    // This would be an internal logic error, since our callers should have made sure we're
                    // on the primitive value after the property name (and the Utf8JsonReader would have complained about any
                    // other token that one that is a value).
                    // EK: I think 'Comment' is the only possible non-expected option here....
                    throw new InvalidOperationException($"Unexpected token type {reader.TokenType} while parsing a primitive value. " +
                        ERR.GenerateLocationMessage(ref reader)),
            };

            // Read past the value
            reader.Read();

            return result;

            static PartialDeserialization<object> recover(ref Utf8JsonReader reader, object? result, JsonFhirException error)
            {
                reader.Recover();
                return new(result, error);
            }

            static PartialDeserialization<object> readBase64(ref Utf8JsonReader reader) =>
                reader.TryGetBytesFromBase64(out var bytesValue) ?
                    new(bytesValue, null) : 
                    new(reader.GetString(), ERR.INCORRECT_BASE64_DATA.With(ref reader));

            static PartialDeserialization<object> readDateTimeOffset(ref Utf8JsonReader reader)
            {
                var contents = reader.GetString()!;

                return ElementModel.Types.DateTime.TryParse(contents, out var parsed) ?
                    new (parsed.ToDateTimeOffset(TimeSpan.Zero), null) :
                    new (contents, ERR.STRING_ISNOTA_DATETIME.With(ref reader, contents));
            }
        }

       
        /// <summary>
        /// This function tries to map from the json-format "generic" number to the kind of numeric type defined in the POCO.
        /// </summary>
        /// <remarks>Reader must be positioned on a number token. This function will not move the reader to the next token.</remarks>
        private static PartialDeserialization<object> tryGetMatchingNumber(ref Utf8JsonReader reader, Type requiredType)
        {
            if (reader.TokenType != JsonTokenType.Number) 
                throw new InvalidOperationException($"Cannot read a numeric when reader is on a {reader.TokenType}. " + ERR.GenerateLocationMessage(ref reader));

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
                success = reader.TryGetSingle(out float si) && float.IsNormal(si) && (value = si) is { };
            else if (requiredType == typeof(double))
                success = reader.TryGetDouble(out double dbl) && double.IsNormal(dbl) && (value = dbl) is { };
            else
            {
                var gotValue = reader.TryGetNumber(out value);
                return new(value, ERR.UNEXPECTED_JSON_TOKEN.With(ref reader, requiredType.Name, "number"));
            }

            // We expected a number, we found a json number, but they don't match (e.g. precision etc)
            if(success)
            {
                return new(value, null);
            }
            else
            {
                var gotValue = reader.TryGetNumber(out value);
                return new(value, ERR.NUMBER_CANNOT_BE_PARSED.With(ref reader, gotValue ? value : "(unreadable number)", requiredType.Name));
            }
        }

        /// <summary>
        /// Returns the <see cref="ClassMapping" /> for the object to be deserialized using the `resourceType` property.
        /// </summary>
        /// <remarks>Assumes the reader is on the start of an object.</remarks>
        internal static PartialDeserialization<ClassMapping> DetermineClassMappingFromInstance(ref Utf8JsonReader reader, ModelInspector inspector)
        {
            var outcome = determineResourceType(ref reader);

            if (outcome.Success)
            {
                var resourceType = outcome.PartialResult!;
                var resourceMapping = inspector.FindClassMapping(resourceType);

                return resourceMapping is not null ? 
                    (new(resourceMapping, null)) :
                    (new(null, ERR.UNKNOWN_RESOURCE_TYPE.With(ref reader, resourceType)));
            }
            else
                return new(null, outcome.Exception);
        }

        private static PartialDeserialization<string> determineResourceType(ref Utf8JsonReader reader)
        {
            //TODO: determineResourceType probably won't work with streaming inputs to Utf8JsonReader                       

            var originalReader = reader;    // copy the struct so we can "rewind"
            var atDepth = reader.CurrentDepth + 1;

            try
            {
                while (reader.Read() && reader.CurrentDepth >= atDepth)
                {
                    if (reader.TokenType == JsonTokenType.PropertyName && reader.CurrentDepth == atDepth)
                    {
                        var propName = reader.GetString();

                        if (propName == "resourceType")
                        {
                            reader.Read();
                            return (reader.TokenType == JsonTokenType.String) ?
                                new(reader.GetString()!, null) :
                                new(null, ERR.RESOURCETYPE_SHOULD_BE_STRING.With(ref reader, reader.TokenType));
                        }
                    }
                }

                return new(null, ERR.NO_RESOURCETYPE_PROPERTY.With(ref reader));
            }
            finally
            {
                reader = originalReader;
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
                ?? throw ERR.UNKNOWN_PROPERTY_FOUND.With(ref reader, propertyName);

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
                    throw ERR.CHOICE_ELEMENT_HAS_NO_TYPE.With(ref r, propertyMapping.Name);

                return inspector.FindClassMapping(typeSuffix) ??
                    throw ERR.CHOICE_ELEMENT_HAS_UNKOWN_TYPE.With(ref r, propertyMapping.Name, typeSuffix);

                //TODO: Check whether the type is allowed here?
            }
        }

        //TODO: check & guard direct casts
    }


}

#nullable restore
#endif