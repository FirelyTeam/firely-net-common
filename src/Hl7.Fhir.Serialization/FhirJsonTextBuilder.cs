/* 
 * Copyright (c) 2018, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/firely-net-sdk/blob/master/LICENSE
 */

#if NETSTANDARD2_0

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Hl7.Fhir.Serialization
{
    internal class FhirJsonTextBuilder : IExceptionSource
    {
        internal FhirJsonTextBuilder(FhirJsonSerializationSettings settings = null)
        {
            _settings = settings?.Clone() ?? new FhirJsonSerializationSettings();
        }

        private FhirJsonSerializationSettings _settings;
        private bool _roundtripMode = true;

        public ExceptionNotificationHandler ExceptionHandler { get; set; }

        public void Build(ITypedElement source, Stream stream) => buildInternal(source, stream);

        public void Build(ISourceNode source, Stream stream)
        {
            bool hasJsonSource = source.Annotation<FhirJsonNode>() != null;

            // We can only work with an untyped source if we're doing a roundtrip,
            // so we have all serialization details available.
            if (hasJsonSource)
            {
                _roundtripMode = true;          // will allow unknown elements to be processed
#pragma warning disable 612, 618
                buildInternal(source.ToTypedElement(), stream);
#pragma warning restore 612, 618
            }
            else
            {
                throw Error.NotSupported($"The {nameof(FhirJsonBuilder)} will only work correctly on an untyped " +
                    $"source if the source is a {nameof(FhirJsonNode)}.");
            }
        }

        private void buildInternal(ITypedElement source, Stream stream)
        {
            var jso = new JsonWriterOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Indented = _settings.Pretty
            };

            using var dest = new Utf8JsonWriter(stream, jso);

            dest.WriteObject(() =>
            {
                if (source is IExceptionSource)
                {
                    using (source.Catch((o, a) => ExceptionHandler.NotifyOrThrow(o, a)))
                    {
                        addChildren(source, dest);
                    }
                }
                else
                    addChildren(source, dest);
            });

            dest.Flush();
        }

        // These are the "primitive" FHIR instance types that possibly need a separate element/_element
        // serialization in json.
        private static readonly string[] primitiveTypes =
        {
            "boolean",
             "integer",
             "integer64",
             "unsignedInt",
             "positiveInt",
             "time",
             "date",
             "instant",
             "dateTime",
             "decimal",
             "string",
             "code",
             "id",
             "uri",
             "oid",
             "uuid",
             "canonical",
             "url",
             "markdown",
             "base64Binary",
             "xhtml"
        };

        internal bool MustSerializeMember(ITypedElement source, out IElementDefinitionSummary info)
        {
            info = source.Definition;

            if (info == null && !_roundtripMode)
            {
                var message = $"Element '{source.Location}' is missing type information.";

                if (_settings.IgnoreUnknownElements)
                {
                    ExceptionHandler.NotifyOrThrow(source, ExceptionNotification.Warning(
                        new MissingTypeInformationException(message)));
                }
                else
                {
                    ExceptionHandler.NotifyOrThrow(source, ExceptionNotification.Error(
                        new MissingTypeInformationException(message)));
                }

                return false;
            }

            return true;
        }

        private void addChildren(ITypedElement node, Utf8JsonWriter writer)
        {
            var resourceTypeIndicator = node.Annotation<IResourceTypeSupplier>()?.ResourceType;
            var isResource = node.Definition?.IsResource ?? resourceTypeIndicator != null;
            var containedResourceType = isResource ? (node.InstanceType ?? resourceTypeIndicator) : null;
            if (containedResourceType is { })
            {
                writer.WriteString(JsonSerializationDetails.RESOURCETYPE_MEMBER_NAME, containedResourceType);
            }

            foreach (var nameGroup in node.Children().GroupBy(n => n.Name))
            {
                var members = nameGroup.ToList();

                // serialization info should be the same for each element in an
                // array - but do not explicitly check that
                if (!MustSerializeMember(members[0], out var generalInfo)) break;
                bool hasTypeInfo = generalInfo != null;

                // If we have type information, we know whather we need an array.
                // failing that, check whether this is a roundtrip and we have the information
                // about arrays in the serialization details. Failing that, assume the default:
                // for unknown properties is to use an array - safest bet.
                var details = members[0].GetJsonSerializationDetails();
                var hasIndex = details?.ArrayIndex != null;
                var needsArray = generalInfo?.IsCollection ?? hasIndex;
                var objectInShadow = members[0].InstanceType != null ? primitiveTypes.Contains(members[0].InstanceType) : details?.UsesShadow ?? false;

                if (!members.Any()) continue;

                var propertyName = generalInfo?.IsChoiceElement == true ?
                       $"{members[0].Name}{members[0].InstanceType.Capitalize()}" : members[0].Name;

                // property has a shadow property, because it has children elements as well
                var shadowInProgress = objectInShadow && members.SelectMany(m => m.Children()).Any();

                if (shadowInProgress)
                {
                    // write first shadow element (_properties)
                    writeShadow(writer, propertyName, members, details, needsArray, out var valueList);
                    if (valueList.Any(n => n is { }))
                    {
                        // shadow property has an existing value
                        writer.WritePropertyName(propertyName);
                        writer.WriteObject(() => writer.WriteValues(valueList), needsArray);
                    }
                }
                else
                {
                    writeNormal(writer, propertyName, members, details, needsArray);
                }
            }
        }

        private void writeShadow(Utf8JsonWriter writer, string propertyName, IEnumerable<ITypedElement> members, JsonSerializationDetails details, bool needsArray, out IList<object> valueList)
        {
            writer.WritePropertyName($"_{propertyName}");

            var list = new List<object>();

            writer.WriteObject(() =>
            {
                foreach (var child in members)
                {
                    object value = child.Definition != null ? child.Value : details?.OriginalValue ?? child.Value;

                    // save the value for later
                    list.Add(value);

                    // does this child has children of its own (like id or extensions)?
                    if (child.Children().Any())
                    {
                        writer.WriteObject(() => addChildren(child, writer));
                    }
                    else
                    {
                        writer.WriteValue(null);
                    }
                }
            }, needsArray);
            valueList = list; // valueList cannot be used inside an anonymous function
        }

        private void writeNormal(Utf8JsonWriter writer, string propertyName, IEnumerable<ITypedElement> members, JsonSerializationDetails details, bool needsArray)
        {
            // No values and no children? Then leave this function
            if (members.All(m => m.Value is null) && !members.SelectMany(m => m.Children()).Any())
                return;

            writer.WritePropertyName(propertyName);
            writer.WriteObject(() =>
            {
                foreach (var child in members)
                {
                    object value = child.Definition != null ? child.Value : details?.OriginalValue ?? child.Value;

                    if (value is null)
                    {
                        if (child.Children().Any())
                        {
                            writer.WriteObject(() => addChildren(child, writer));
                        }
                    }
                    else
                    {
                        writer.WriteValue(value);
                    }
                }
            }, needsArray);
        }
    }

    internal static class Utf8JsonWriterExtension
    {
        public static void WriteObject(this Utf8JsonWriter writer, Action action)
        {
            writer.WriteStartObject();
            action();
            writer.WriteEndObject();
        }

        public static void WriteObject(this Utf8JsonWriter writer, Action action, bool useArray)
        {
            if (useArray)
                writer.WriteStartArray();
            action();
            if (useArray)
                writer.WriteEndArray();
        }

        public static void WriteValue(this Utf8JsonWriter writer, object value)
        {
            switch (value)
            {
                case null:
                    writer.WriteNullValue();
                    break;
                case bool b:
                    writer.WriteBooleanValue(b);
                    break;
                case decimal d:
                    writer.WriteNumberValue(d);
                    break;
                case Int32 i32:
                    writer.WriteNumberValue(i32);
                    break;
                case Int16 i16:
                    writer.WriteNumberValue(i16);
                    break;
                case ulong ul:
                    writer.WriteNumberValue(ul);
                    break;
                case double db:
                    writer.WriteNumberValue(db);
                    break;
                // TODO: no default write method for BigInteger
                // case BigInteger bi:
                //     writer.WriteNumberValue(bi);
                //     break;
                case float f:
                    writer.WriteNumberValue(f);
                    break;
                case string s:
                    writer.WriteStringValue(s.Trim());
                    break;
                case long l:
                default:
                    writer.WriteStringValue(PrimitiveTypeConverter.ConvertTo<string>(value));
                    break;
            }
        }

        public static void WriteValues(this Utf8JsonWriter writer, IEnumerable<object> values)
        {
            foreach (var value in values)
            {
                writer.WriteValue(value);
            }
        }
    }
}
#endif