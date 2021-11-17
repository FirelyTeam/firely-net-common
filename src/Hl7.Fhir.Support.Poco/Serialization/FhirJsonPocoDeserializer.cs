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
using Hl7.Fhir.Utility;
using Hl7.Fhir.Validation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using ERR = Hl7.Fhir.Serialization.FhirJsonException;

#nullable enable

namespace Hl7.Fhir.Serialization
{
    /// <summary>
    /// Deserializes a byte stream into FHIR POCO objects.
    /// </summary>
    /// <remarks>The serializer uses the format documented in https://www.hl7.org/fhir/json.html. </remarks>
    public class FhirJsonPocoDeserializer
    {
        /// <summary>
        /// Initializes an instance of the deserializer.
        /// </summary>
        /// <param name="assembly">Assembly containing the POCO classes to be used for deserialization.</param>
        public FhirJsonPocoDeserializer(Assembly assembly) : this(assembly, new())
        {
            // nothing
        }

        /// <summary>
        /// Initializes an instance of the deserializer.
        /// </summary>
        /// <param name="assembly">Assembly containing the POCO classes to be used for deserialization.</param>
        /// <param name="settings">A settings object to be used by this instance.</param>
        public FhirJsonPocoDeserializer(Assembly assembly, FhirJsonPocoDeserializerSettings settings)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            Settings = settings;
            _inspector = ModelInspector.ForAssembly(assembly);
        }

        /// <summary>
        /// Assembly containing the POCO classes the deserializer will use to deserialize data into.
        /// </summary>
        public Assembly Assembly { get; }

        /// <summary>
        /// The options that were set by the constructor.
        /// </summary>
        public FhirJsonPocoDeserializerSettings Settings { get; }

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

            ExceptionAggregator aggregator = new();

            var result = DeserializeResourceInternal(ref reader, aggregator);

            return !aggregator.HasExceptions
                ? result!
                : throw new DeserializationFailedException(result, aggregator);
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
                    $"therefore not be used for deserialization. " + reader.GenerateLocationMessage(), nameof(targetType));

            // Create a new instance of the object to read the members into.
            if (mapping.Factory() is Base result)
            {
                var aggregator = new ExceptionAggregator();
                DeserializeObjectInto(result, mapping, ref reader, inResource: false, aggregator);
                return !aggregator.HasExceptions
                    ? result
                    : throw new DeserializationFailedException(result, aggregator);
            }
            else
                throw new ArgumentException($"Can only deserialize into subclasses of class {nameof(Base)}. " + reader.GenerateLocationMessage(), nameof(targetType));
        }

        /// <summary>
        /// Reads a (subtree) of serialzed FHIR Json data into a POCO object.
        /// </summary>
        /// <typeparam name="T">The type of POCO to construct and deserialize</typeparam>
        /// <param name="reader">A json reader positioned on the first token of the object, or the beginning of the stream.</param>
        /// <returns>A fully initialized POCO with the data from the reader.</returns>
        public T DeserializeObject<T>(ref Utf8JsonReader reader) where T : Base => (T)DeserializeObject(typeof(T), ref reader);

        internal Resource? DeserializeResourceInternal(ref Utf8JsonReader reader, ExceptionAggregator aggregator)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                aggregator.Add(ERR.EXPECTED_START_OF_OBJECT.With(ref reader, reader.TokenType));
                reader.Recover();  // skip to the end of the construct encountered (value or array)                
                return null;
            }

            (ClassMapping? resourceMapping, FhirJsonException? error) = DetermineClassMappingFromInstance(ref reader, _inspector);

            if (resourceMapping is not null)
            {
                // If we have at least a mapping, let's try to continue               
                var newResource = (Base)resourceMapping.Factory();
                DeserializeObjectInto(newResource, resourceMapping, ref reader, inResource: true, aggregator);

                if (!resourceMapping.IsResource)
                {
                    aggregator.Add(ERR.RESOURCE_TYPE_NOT_A_RESOURCE.With(ref reader, resourceMapping.Name));
                    return null;
                }
                else
                    return (Resource)newResource;
            }
            else
            {
                aggregator.Add(error!);

                // Read past the end of this object to recover.
                reader.Recover();

                return null;
            }
        }

        /// <summary>
        /// Reads a complex object into an existing instance of a POCO.
        /// </summary>
        /// <remarks>Reader will be on the first token after the object upon return.</remarks>
        internal void DeserializeObjectInto<T>(T target, ClassMapping mapping, ref Utf8JsonReader reader, bool inResource, ExceptionAggregator aggregator)
            where T : Base
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                aggregator.Add(ERR.EXPECTED_START_OF_OBJECT.With(ref reader, reader.TokenType));
                reader.Recover();  // skip to the end of the construct encountered (value or array)
                return;
            }

            // read past start of object into first property or end of object
            reader.Read();

            var empty = true;
            var plps = new FhirPrimitiveListParsingState();

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                var currentPropertyName = reader.GetString()!;

                if (currentPropertyName == "resourceType")
                {
                    if (!inResource) aggregator.Add(ERR.RESOURCETYPE_UNEXPECTED.With(ref reader));
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
                catch (FhirJsonException jfe)
                {
                    aggregator.Add(jfe);

                    // try to recover by skipping to the next property.
                    reader.SkipTo(JsonTokenType.PropertyName);
                    continue;
                }

                // read past the property name into the value
                reader.Read();

                deserializePropertyValueInto(target, currentPropertyName, propMapping, propValueMapping, ref reader, plps, aggregator);
            }

            // check for single array properties containing a null
            aggregator.Add(plps.Check(ref reader));

            // read past object
            reader.Read();

            // do not allow empty complex objects.
            if (empty) aggregator.Add(ERR.OBJECTS_CANNOT_BE_EMPTY.With(ref reader));

            return;
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
        /// <param name="aggregator">Object used to collect parsing errors.</param>
        /// <remarks>Expects the reader to be positioned on the property value.
        /// Reader will be on the first token after the property value upon return.</remarks>
        private void deserializePropertyValueInto(
            Base target,
            string propertyName,
            PropertyMapping propertyMapping,
            ClassMapping propertyValueMapping,
            ref Utf8JsonReader reader,
            FhirPrimitiveListParsingState plps,
            ExceptionAggregator aggregator
            )
        {
            object? result;
            var oldErrorCount = aggregator.Count;

            if (propertyValueMapping.IsFhirPrimitive)
            {
                // There might be an existing value, since FhirPrimitives may be spread out over two properties
                // (one with, and one without the '_')
                var existingValue = propertyMapping.GetValue(target);

                // Note that the POCO model will always allocate a new list if the property had not been set before,
                // so there is always an existingValue for IList                
                result = propertyMapping.IsCollection ?
                    deserializeFhirPrimitiveList((IList)existingValue!, propertyName, propertyValueMapping, ref reader, plps, aggregator) :
                    DeserializeFhirPrimitive(existingValue as PrimitiveType, propertyName, propertyValueMapping, ref reader, aggregator);
            }
            else
            {
                // This is not a FHIR primitive, so we should not be dealing with these weird _name members.
                if (propertyName[0] == '_')
                    aggregator.Add(ERR.USE_OF_UNDERSCORE_ILLEGAL.With(ref reader, propertyMapping.Name, propertyName));

                // Note that repeating simple elements (like Extension.url) do not currently exist in the FHIR serialization
                result = propertyMapping.IsCollection
                    ? deserializeNormalList(propertyName, propertyValueMapping, ref reader, aggregator)
                    : deserializeSingleValue(ref reader, propertyValueMapping, aggregator);
            }

            propertyMapping.SetValue(target, result);

            // Only do validation when no parse errors were encountered, otherwise we'll just
            // produce spurious messages.
            if (oldErrorCount == aggregator.Count)
                validateValue(ref reader, result, propertyMapping, aggregator);

            return;
        }


        private static void validateValue(ref Utf8JsonReader reader, object? value, PropertyMapping propertyMapping, ExceptionAggregator aggregator)
        {
            foreach (var va in propertyMapping.ValidationAttributes)
            {
                // The ElementAttribute does not add to this elements validation - it's only used
                // to extend .NETs property validation into nested values (which we have already validated
                // while parsing bottom up).
                if (va is not FhirElementAttribute)
                {
                    var validationResult = DotNetAttributeValidation.GetValidationResult(value, va);
                    if (validationResult is not null)
                        aggregator.Add(ERR.VALIDATION_FAILED.With(ref reader, validationResult.ErrorMessage));
                }
            }
        }

        /// <summary>
        /// Reads the content of a list with non-FHIR-primitive content (so, no name/_name pairs to be dealt with). Note
        /// that the contents can only be complex in the current FHIR serialization, but we'll be prepared and handle
        /// other situations (e.g. repeating Extension.url's, if they would ever exist).
        /// </summary>
        private IList? deserializeNormalList(
            string propertyName,
            ClassMapping propertyValueMapping,
            ref Utf8JsonReader reader,
            ExceptionAggregator aggregator)
        {
            // Create a list of the type of this property's value.
            var listInstance = propertyValueMapping.ListFactory();

            // if true, we have encountered a single value where we expected an array.
            // we need to recover by creating an array with that single value.
            bool oneshot = false;

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                aggregator.Add(ERR.EXPECTED_START_OF_ARRAY.With(ref reader, propertyName));
                oneshot = true;
            }
            else
            {
                // Read past start of array
                reader.Read();

                if (reader.TokenType == JsonTokenType.EndArray)
                    aggregator.Add(ERR.ARRAYS_CANNOT_BE_EMPTY.With(ref reader));
            }

            // Can't make an iterator because of the ref readers struct, so need
            // to simply create a list by Adding(). Not the fastest approach :-(
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                var result = deserializeSingleValue(ref reader, propertyValueMapping, aggregator);
                listInstance.Add(result);

                if (oneshot) break;
            }

            // Read past end of array
            if (!oneshot) reader.Read();

            return listInstance;
        }

        private class FhirPrimitiveListParsingState
        {
            public Dictionary<string, string?> SingleArraysWithNull = new();

            public FhirJsonException? Check(ref Utf8JsonReader reader)
            {
                var relevantElements = SingleArraysWithNull.Where(kvp => kvp.Value is not null).Select(kvp => kvp.Key);

                return relevantElements.Any()
                    ? ERR.PRIMITIVE_ARRAYS_LONELY_NULL.With(ref reader, string.Join(", ", relevantElements))
                    : null;
            }
        }

        /// <summary>
        /// Reads a list of FHIR primitives (either from a name or _name property).
        /// </summary>
        /// <remarks>Upon completion, reader will be located at the next token afther the list.</remarks>
        private IList? deserializeFhirPrimitiveList(
            IList existingList,
            string propertyName,
            ClassMapping propertyValueMapping,
            ref Utf8JsonReader reader,
            FhirPrimitiveListParsingState state,
            ExceptionAggregator aggregator
            )
        {
            // true if we have already encountered this property and it used the 'null' feature in primitive arrays
            bool hadLonely = state.SingleArraysWithNull.Remove(propertyName.TrimStart('_'));

            // if true, we have encountered a single value where we expected an array.
            // we need to recover by creating an array with that single value.
            bool oneshot = false;

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                aggregator.Add(ERR.EXPECTED_START_OF_ARRAY.With(ref reader, propertyName));
                oneshot = true;
            }
            else
            {
                // read into array
                reader.Read();

                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    aggregator.Add(ERR.ARRAYS_CANNOT_BE_EMPTY.With(ref reader));

                    // Make sure we don't incorrectly report an empty array
                    // as using the null feature incorrectly.
                    if (!hadLonely) state.SingleArraysWithNull.Add(propertyName.TrimStart('_'), null);
                }
            }

            int originalSize = existingList.Count;

            // Can't make an iterator because of the ref readers struct, so need
            // to simply create a list by Adding(). Not the fastest approach :-(
            int elementIndex = 0;
            bool? onlyNulls = null;

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (elementIndex >= originalSize)
                    existingList.Add(propertyValueMapping.Factory());

                var targetPrimitive = (PrimitiveType)existingList[elementIndex]!;

                if (reader.TokenType == JsonTokenType.Null)
                {
                    if (onlyNulls is null) onlyNulls = true;

                    if (originalSize == 0 && !hadLonely) state.SingleArraysWithNull.TryAdd(propertyName.TrimStart('_'), propertyName);

                    if (originalSize > 0 && elementIndex < originalSize && !targetPrimitive.Any())
                        aggregator.Add(ERR.PRIMITIVE_ARRAYS_BOTH_NULL.With(ref reader));

                    // don't read any new data into the primitive here
                    reader.Read();
                }
                else
                {
                    onlyNulls = false;
                    _ = DeserializeFhirPrimitive(targetPrimitive, propertyName, propertyValueMapping, ref reader, aggregator);
                }

                elementIndex += 1;

                if (oneshot) break;
            }

            if (onlyNulls == true)
            {
                aggregator.Add(ERR.PRIMITIVE_ARRAYS_ONLY_NULL.With(ref reader, propertyName));
            }
            if (originalSize > 0 && elementIndex != originalSize)
                aggregator.Add(ERR.PRIMITIVE_ARRAYS_INCOMPAT_SIZE.With(ref reader));

            // read past array to next property or end of object
            if (!oneshot) reader.Read();

            return existingList;
        }

        /// <summary>
        /// Deserializes a FHIR primitive, which can be a name or _name property.
        /// </summary>
        /// <remarks>Upon completion, reader will be located at the next token afther the FHIR primitive.</remarks>
        internal PrimitiveType DeserializeFhirPrimitive(
            PrimitiveType? existingPrimitive,
            string propertyName,
            ClassMapping propertyValueMapping,
            ref Utf8JsonReader reader,
            ExceptionAggregator aggregator
            )
        {
            var targetPrimitive = existingPrimitive ?? (PrimitiveType)propertyValueMapping.Factory();

            if (propertyName[0] != '_')
            {
                // No underscore, dealing with the 'value' property here.
                var primitiveValueProperty = propertyValueMapping.PrimitiveValueProperty ??
                    throw new InvalidOperationException($"All subclasses of {nameof(PrimitiveType)} should have a value property, " +
                        $"but {propertyValueMapping.Name} has not. " + reader.GenerateLocationMessage());

                var (result, error) = DeserializePrimitiveValue(ref reader, Settings.OnPrimitiveParseFailed, primitiveValueProperty.ImplementingType);
                targetPrimitive.ObjectValue = result;

                aggregator.Add(error);

                // Only do validation when no parse errors were encountered, otherwise we'll just
                // produce spurious messages.
                if (error is null)
                    validateValue(ref reader, result, primitiveValueProperty, aggregator);

                return targetPrimitive;
            }
            else
            {
                // The complex part of a primitive - read the object's primitives into the target
                DeserializeObjectInto(targetPrimitive, propertyValueMapping, ref reader, inResource: false, aggregator);
                return targetPrimitive;
            }
        }

        /// <summary>
        /// Deserializes a single object, either a resource, a FHIR primitive or a primitive value.
        /// </summary>
        /// <remarks>Upon completion, reader will be located at the next token afther the value.</remarks>
        private object? deserializeSingleValue(ref Utf8JsonReader reader, ClassMapping propertyValueMapping, ExceptionAggregator aggregator)
        {
            // Resources
            if (propertyValueMapping.IsResource)
            {
                return DeserializeResourceInternal(ref reader, aggregator);
            }

            // primitive values (not FHIR primitives, real primitives, like Element.id)
            // Note: 'value' attributes for FHIR primitives are handled elsewhere, since that logic
            // needs to handle PrimitiveType.ObjectValue & dual properties.
            else if (propertyValueMapping.IsPrimitive)
            {
                var (result, error) = DeserializePrimitiveValue(ref reader, Settings.OnPrimitiveParseFailed, propertyValueMapping.NativeType);

                if (error is not null && result is not null)
                {
                    // Signal the fact that we're throwing away data here, as we cannot put
                    // "raw" data into a simple property like Id and Url.
                    aggregator.Add(ERR.INCOMPATIBLE_SIMPLE_VALUE.With(ref reader, error, error.Message));
                    return null;
                }
                else
                {
                    aggregator.Add(error);
                    return result;
                }
            }

            // "normal" complex types & backbones
            else
            {
                var newComplex = (Base)propertyValueMapping.Factory();
                DeserializeObjectInto(newComplex, propertyValueMapping, ref reader, inResource: false, aggregator);
                return newComplex;
            }
        }

        /// <summary>
        /// Does a best-effort parse of the data available at the reader, given the required type of the property the
        /// data needs to be read into. 
        /// </summary>
        /// <returns>A value without an error if the data could be parsed to the required type, and a value with an error if the
        /// value could not be parsed - in which case the value returned is the raw value coming in from the reader.</returns>
        /// <remarks>Upon completion, the reader will be positioned on the token after the primitive.</remarks>
        static internal (object?, FhirJsonException?) DeserializePrimitiveValue(ref Utf8JsonReader reader, PrimitiveParseHandler? recovery, Type requiredType)
        {
            // Check for unexpected non-value types.
            if (reader.TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray)
            {
                var exception = reader.TokenType == JsonTokenType.StartObject
                    ? ERR.EXPECTED_PRIMITIVE_NOT_OBJECT.With(ref reader)
                    : ERR.EXPECTED_PRIMITIVE_NOT_ARRAY.With(ref reader);
                reader.Recover();
                return (null, exception);
            }

            // Check for value types
            (object? partial, FhirJsonException? error) result = reader.TokenType switch
            {
                JsonTokenType.Null => new(null, ERR.EXPECTED_PRIMITIVE_NOT_NULL.With(ref reader)),
                JsonTokenType.String when requiredType == typeof(string) => new(reader.GetString(), null),
                JsonTokenType.String when requiredType == typeof(byte[]) => readBase64(ref reader),
                JsonTokenType.String when requiredType == typeof(DateTimeOffset) => readDateTimeOffset(ref reader),
                JsonTokenType.String when requiredType.IsEnum => readEnum(ref reader, requiredType),
                JsonTokenType.String => unexpectedToken(ref reader, reader.GetString(), requiredType.Name, "string"),
                JsonTokenType.Number => tryGetMatchingNumber(ref reader, requiredType),
                JsonTokenType.True or JsonTokenType.False when requiredType == typeof(bool) => new(reader.GetBoolean(), null),
                JsonTokenType.True or JsonTokenType.False => unexpectedToken(ref reader, reader.GetRawText(), requiredType.Name, "boolean"),

                _ =>
                    // This would be an internal logic error, since our callers should have made sure we're
                    // on the primitive value after the property name (and the Utf8JsonReader would have complained about any
                    // other token that one that is a value).
                    // EK: I think 'Comment' is the only possible non-expected option here....
                    throw new InvalidOperationException($"Unexpected token type {reader.TokenType} while parsing a primitive value. " +
                        reader.GenerateLocationMessage()),
            };


            // If there is a failure, and we have a handler installed, call it
            if (recovery is not null && result.error is not null)
            {
                try
                {
                    var newPartial = recovery(ref reader, requiredType, result.error);
                    result = (newPartial, null);
                }
                catch (FhirJsonException fje)
                {
                    result = (result.partial, fje);
                }
            }

            // Read past the value
            reader.Read();

            return result;

            static (object?, FhirJsonException?) readBase64(ref Utf8JsonReader reader) =>
                reader.TryGetBytesFromBase64(out var bytesValue) ?
                    new(bytesValue, null) :
                    new(reader.GetString(), ERR.INCORRECT_BASE64_DATA.With(ref reader));

            static (object?, FhirJsonException?) readDateTimeOffset(ref Utf8JsonReader reader)
            {
                var contents = reader.GetString()!;

                return ElementModel.Types.DateTime.TryParse(contents, out var parsed) ?
                    new(parsed.ToDateTimeOffset(TimeSpan.Zero), null) :
                    new(contents, ERR.STRING_ISNOTAN_INSTANT.With(ref reader, contents));
            }

            static (object?, FhirJsonException?) readEnum(ref Utf8JsonReader reader, Type enumType)
            {
                var contents = reader.GetString()!;
                var enumValue = EnumUtility.ParseLiteral(contents, enumType);

                return enumValue is not null
                    ? (contents, null)
                    : (contents, ERR.CODED_VALUE_NOT_IN_ENUM.With(ref reader, contents, EnumUtility.GetName(enumType)));
            }
        }

        private static (object?, FhirJsonException) unexpectedToken(ref Utf8JsonReader reader, string? value, string expected, string actual) =>
            new(value, ERR.UNEXPECTED_JSON_TOKEN.With(ref reader, expected, actual, value));

        /// <summary>
        /// This function tries to map from the json-format "generic" number to the kind of numeric type defined in the POCO.
        /// </summary>
        /// <remarks>Reader must be positioned on a number token. This function will not move the reader to the next token.</remarks>
        private static (object?, FhirJsonException?) tryGetMatchingNumber(ref Utf8JsonReader reader, Type requiredType)
        {
            if (reader.TokenType != JsonTokenType.Number)
                throw new InvalidOperationException($"Cannot read a numeric when reader is on a {reader.TokenType}. " +
                    reader.GenerateLocationMessage());

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
                var rawValue = reader.GetRawText();
                return unexpectedToken(ref reader, rawValue, requiredType.Name, "number");
            }

            // We expected a number, we found a json number, but they don't match (e.g. precision etc)
            if (success)
            {
                return new(value, null);
            }
            else
            {
                var rawValue = reader.GetRawText();
                return new(rawValue, ERR.NUMBER_CANNOT_BE_PARSED.With(ref reader, rawValue, requiredType.Name));
            }
        }

        /// <summary>
        /// Returns the <see cref="ClassMapping" /> for the object to be deserialized using the `resourceType` property.
        /// </summary>
        /// <remarks>Assumes the reader is on the start of an object.</remarks>
        internal static (ClassMapping?, FhirJsonException?) DetermineClassMappingFromInstance(ref Utf8JsonReader reader, ModelInspector inspector)
        {
            var (resourceType, error) = determineResourceType(ref reader);

            if (resourceType is not null)
            {
                var resourceMapping = inspector.FindClassMapping(resourceType);

                return resourceMapping is not null ?
                    (new(resourceMapping, null)) :
                    (new(null, ERR.UNKNOWN_RESOURCE_TYPE.With(ref reader, resourceType)));
            }
            else
                return new(null, error);
        }

        private static (string?, FhirJsonException?) determineResourceType(ref Utf8JsonReader reader)
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

                var typeSuffixMapping = string.IsNullOrEmpty(typeSuffix)
                    ? throw ERR.CHOICE_ELEMENT_HAS_NO_TYPE.With(ref r, propertyMapping.Name)
                    : inspector.FindClassMapping(typeSuffix) ??
                    throw ERR.CHOICE_ELEMENT_HAS_UNKOWN_TYPE.With(ref r, propertyMapping.Name, typeSuffix);

                if (!propertyMapping.FhirType.Any(ft => ft.IsAssignableFrom(typeSuffixMapping.NativeType)))
                    throw ERR.CHOICE_ELEMENT_TYPE_NOT_ALLOWED.With(ref r, propertyMapping.Name, typeSuffix);

                return typeSuffixMapping;
            }
        }
    }
}

#nullable restore
#endif