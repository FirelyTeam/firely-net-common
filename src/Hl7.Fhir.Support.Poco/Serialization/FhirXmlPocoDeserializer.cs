#nullable enable

using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Xml;
using ERR = Hl7.Fhir.Serialization.FhirXmlException;

namespace Hl7.Fhir.Serialization
{
    public class FhirXmlPocoDeserializer
    {
        /// <summary>
        /// Initializes an instance of the deserializer.
        /// </summary>
        /// <param name="assembly">Assembly containing the POCO classes to be used for deserialization.</param>
        public FhirXmlPocoDeserializer(Assembly assembly) : this(assembly, new())
        {
            // nothing
        }

        /// <summary>
        /// Initializes an instance of the deserializer.
        /// </summary>
        /// <param name="assembly">Assembly containing the POCO classes to be used for deserialization.</param>
        /// <param name="settings">A settings object to be used by this instance.</param>
        public FhirXmlPocoDeserializer(Assembly assembly, FhirXmlPocoDeserializerSettings settings)
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
        /// The settings that were passed to the constructor.
        /// </summary>
        public FhirXmlPocoDeserializerSettings Settings { get; }

        private readonly ModelInspector _inspector;

        /// <summary>
        /// Deserialize the FHIR xml from the reader and create a new POCO object containing the data from the reader.
        /// </summary>
        /// <param name="reader">A xml reader positioned on the element of the object, or the beginning of the stream.</param>
        /// <returns>A fully initialized POCO with the data from the reader.</returns>
        public Resource DeserializeResource(XmlReader reader)
        {
            // If the stream has just been opened, move to the first token. (skip processing instructions, comments, whitespaces etc.)
            reader.MoveToContent();

            FhirXmlPocoDeserializerState state = new();

            var result = DeserializeResourceInternal(reader, state);

            return !state.Errors.HasExceptions
                ? result!
                : throw new DeserializationFailedException(result, state.Errors);

        }


        public Base DeserializeDatatype(Type targetType, XmlReader reader)
        {
            // If the stream has just been opened, move to the first token. (skip processing instructions, comments, whitespaces etc.)
            reader.MoveToContent();

            FhirXmlPocoDeserializerState state = new();

            var result = DeserializeDatatypeInternal(targetType, reader, state);

            return !state.Errors.HasExceptions
                ? result!
                : throw new DeserializationFailedException(result, state.Errors);
        }

        internal Resource? DeserializeResourceInternal(XmlReader reader, FhirXmlPocoDeserializerState state)
        {
            (ClassMapping? resourceMapping, FhirXmlException? error) = DetermineClassMappingFromInstance(reader, _inspector);

            state.Errors.Add(error);

            if (resourceMapping is not null)
            {
                // If we have at least a mapping, let's try to continue               
                var newResource = (Base)resourceMapping.Factory();

                try
                {
                    state.Path.EnterResource(resourceMapping.Name);
                    deserializeDatatypeInto(newResource, resourceMapping, reader, state);

                    if (!resourceMapping.IsResource)
                    {
                        state.Errors.Add(ERR.RESOURCE_TYPE_NOT_A_RESOURCE.With(reader, resourceMapping.Name));
                        return null;
                    }
                    else
                        return (Resource)newResource;
                }
                finally
                {
                    state.Path.ExitResource();
                }
            }
            else
            {
                return null;
            }
        }

        internal Base? DeserializeDatatypeInternal(Type targetType, XmlReader reader, FhirXmlPocoDeserializerState state)
        {
            var mapping = _inspector.FindOrImportClassMapping(targetType) ??
              throw new ArgumentException($"Type '{targetType}' could not be located in model assembly '{Assembly}' and can " +
                  $"therefore not be used for deserialization. " + reader.GenerateLocationMessage(), nameof(targetType));


            if (mapping is not null)
            {
                // If we have at least a mapping, let's try to continue               
                var newDatatype = (Base)mapping.Factory();
                deserializeDatatypeInto(newDatatype, mapping, reader, state);
                return newDatatype;
            }
            return null;
        }

        // We expect to start at the open tag op the element. When done, the reader will be at the next token after this element or end of the file.
        private void deserializeDatatypeInto(Base target, ClassMapping mapping, XmlReader reader, FhirXmlPocoDeserializerState state)
        {
            state.Path.EnterElement(mapping.Name);

            //check if on opening tag
            if (reader.NodeType != XmlNodeType.Element)
            {
                //TODO: throw exception, we have made a mistake
                throw new InvalidOperationException($"Xml node {reader.Name} is not an element, but a {reader.NodeType}");
            }

            if (reader.HasAttributes)
            {
                readAttributes(target, mapping, reader, state);
            }

            //Empty elements have no children e.g. <foo value="bar/>)
            if (!reader.IsEmptyElement)
            {
                //read the next object
                reader.Read();

                int highestOrder = 0;

                while (reader.NodeType != XmlNodeType.EndElement)
                {

                    if (!shouldSkipNodeType(reader.NodeType))
                    {
                        // check if we are currently on an element.
                        var (propMapping, propValueMapping, error) = tryGetMappedElementMetadata(_inspector, mapping, reader, reader.Name);
                        state.Errors.Add(error);

                        if (propMapping is not null)
                        {
                            var incorrectOrder = false;

                            //check if element is in the correct order.
                            if (propMapping.Order >= highestOrder)
                            {
                                highestOrder = propMapping.Order;
                            }
                            else
                            {
                                state.Errors.Add(ERR.UNEXPECTED_ELEMENT.With(reader, reader.Name));
                                incorrectOrder = true;
                            }

                            //check if element is narrative
                            if (propMapping.SerializationHint == Specification.XmlRepresentation.XHtml)
                            {
                                var newValue = readXhtml(propValueMapping, reader, state);
                                propMapping!.SetValue(target, newValue);
                            }
                            //check propMapping if list -> readList or readSingle value 
                            else if (propMapping.IsCollection == true)
                            {
                                var newCollection = createOrExpandList(target, incorrectOrder, propValueMapping!, propMapping, reader, state);
                                propMapping!.SetValue(target, newCollection);

                            }
                            else
                            {
                                var newValue = readSingleValue(propValueMapping!, reader, state);
                                propMapping!.SetValue(target, newValue);
                            }
                        }
                        else
                        {
                            //we don't know this property: error is already thrown in "tryGetMappedElementMetadata(_inspector, mapping, reader, name)";
                            reader.Skip();
                        }
                    }
                }
            }
            reader.Read();
            state.Path.ExitElement();
        }

        private string readXhtml(ClassMapping? propValueMapping, XmlReader reader, FhirXmlPocoDeserializerState state)
        {
            if (reader.NamespaceURI != "http://www.w3.org/1999/xhtml")
            {
                state.Errors.Add(ERR.INCORRECT_XHTML_NAMESPACE.With(reader));
            }
            var xhtml = reader.ReadInnerXml();
            return xhtml;
        }

        //Will create a new list, or adds encountered values to an already existing list (and reports a user error).
        private IList? createOrExpandList(Base target, bool expandCandidate, ClassMapping propValueMapping, PropertyMapping propMapping, XmlReader reader, FhirXmlPocoDeserializerState state)
        {
            //only check for previously created list if the element is in the incorrect place.
            if (expandCandidate)
            {
                var currentList = (IList?)propMapping.GetValue(target);
                //Was there already a list created previously? -> User error!
                //But let's fix it, and expand the list with the newly encountered element(s).
                //Error is already thrown using an "Unexpected element" error in "deserializeDatatypeInto"
                return (currentList!.Count != 0) ? expandCurrentList(currentList, propValueMapping, reader, state) : readList(propValueMapping!, reader, state);
            }
            else
            {
                return readList(propValueMapping!, reader, state);
            }
        }

        //Retrieves previously created list, and add newly encountered values.
        private IList expandCurrentList(IList currentEntries, ClassMapping propValueMapping, XmlReader reader, FhirXmlPocoDeserializerState state)
        {
            var newEntries = readList(propValueMapping!, reader, state);

            if (newEntries != null)
            {
                foreach (var entry in newEntries)
                {
                    currentEntries.Add(entry);
                }
            }
            return currentEntries;
        }

        //When done, the reader will be at the next token after the last element of the list or end of the file.
        private IList? readList(ClassMapping propValueMapping, XmlReader reader, FhirXmlPocoDeserializerState state)
        {
            var list = propValueMapping.ListFactory();

            return propValueMapping.IsResource
                ? readResourceList(reader, state, list)
                : readDatatypeList(propValueMapping, reader, state, list);
        }

        private IList readDatatypeList(ClassMapping propValueMapping, XmlReader reader, FhirXmlPocoDeserializerState state, IList list)
        {
            var name = reader.Name;

            while (reader.Name == name && reader.NodeType != XmlNodeType.EndElement)
            {
                var newEntry = (Base)propValueMapping.Factory();
                deserializeDatatypeInto(newEntry, propValueMapping, reader, state);
                list.Add(newEntry);
            }
            return list;
        }

        private IList readResourceList(XmlReader reader, FhirXmlPocoDeserializerState state, IList list)
        {
            var name = reader.Name;
            var depth = reader.Depth;

            while (reader.Name == name && reader.NodeType != XmlNodeType.EndElement)
            {
                var containedList = new List<Resource?>() { };
                reader.Read(); // move to first child; e.g. from <contained> to the actual resource;       
                if (reader.NodeType != XmlNodeType.EndElement)
                {
                    //read all resources in the resource container, even if there are multiple (which is not allowed)
                    while (reader.Depth != depth && reader.NodeType != XmlNodeType.EndElement)
                    {
                        var resource = DeserializeResourceInternal(reader, state);
                        if (resource != null)
                        {
                            containedList.Add(resource);
                        }
                    }
                }
                if (containedList.Count > 1)
                {
                    state.Errors.Add(ERR.MULTIPLE_RESOURCES_IN_RESOURCE_CONTAINER.With(reader));
                }
                containedList.ForEach(r => list.Add(r));

                //move from endTag of resource container to next element, which can be another resource container ofcourse.
                reader.Read();
            }

            return list;
        }

        private Base? readSingleValue(ClassMapping propValueMapping, XmlReader reader, FhirXmlPocoDeserializerState state)
        {
            if (propValueMapping.IsResource)
            {
                return DeserializeResourceInternal(reader, state);
            }
            else
            {
                var newDatatype = (Base)propValueMapping.Factory();
                deserializeDatatypeInto(newDatatype, propValueMapping, reader, state);
                return newDatatype;
            }

        }

        private void readAttributes(Base target, ClassMapping propValueMapping, XmlReader reader, FhirXmlPocoDeserializerState state)
        {
            //move into first attribute
            if (reader.MoveToFirstAttribute())
            {
                try
                {
                    do
                    {
                        var propMapping = propValueMapping.FindMappedElementByName(reader.Name);
                        if (propMapping is not null)
                        {
                            readAttribute(target, propMapping!, reader, state);
                        }
                        else
                        {
                            if (reader.Name != "xmlns") // attribute is not a FHIR attribute, and not a namespace;
                            {
                                state.Errors.Add(ERR.UNKNOWN_ATTRIBUTE.With(reader, reader.Name));
                            }
                        }

                    } while (reader.MoveToNextAttribute());
                }
                finally
                {
                    //move reader back to element so it can continue later
                    reader.MoveToElement();
                }
            }
        }

        ///Parse current attribute value to set the value property of the target.
        private void readAttribute(Base target, PropertyMapping propMapping, XmlReader reader, FhirXmlPocoDeserializerState state)
        {
            //parse current attribute to expected type
            var (parsedValue, error) = ParsePrimitiveValue(reader, propMapping.ImplementingType);

            state.Errors.Add(error);

            if (parsedValue != null)
            {
                if (target is PrimitiveType primitive)
                    primitive.ObjectValue = parsedValue;
                else
                {
                    propMapping.SetValue(target, parsedValue);
                }
            }
        }

        internal (object?, FhirXmlException?) ParsePrimitiveValue(XmlReader reader, Type implementingType)
        {
            if (implementingType == typeof(string))
                return (reader.Value, null);
            else if (implementingType == typeof(bool))
            {
                return bool.TryParse(reader.Value, out var parsed)
                    ? (parsed, null)
                    : (reader.Value, ERR.STRING_ISNOTA_BOOLEAN.With(reader, reader.Value));
            }
            else if (implementingType == typeof(DateTimeOffset))
            {
                return ElementModel.Types.DateTime.TryParse(reader.Value, out var parsed)
                    ? (parsed.ToDateTimeOffset(TimeSpan.Zero), null)
                    : (reader.Value, ERR.STRING_ISNOTAN_INSTANT.With(reader, reader.Value));
            }
            else if (implementingType == typeof(byte[]))
            {
                return !Settings.DisableBase64Decoding ? getByteArrayValue(reader) : ((object?, ERR?))(reader.Value, null);
            }
            else if (implementingType == typeof(int))
            {
                return int.TryParse(reader.Value, out var parsed) ? (parsed, null) : (reader.Value, ERR.STRING_ISNOTAN_INT.With(reader, reader.Value));
            }
            else if (implementingType == typeof(uint))
            {
                return uint.TryParse(reader.Value, out var parsed) ? (parsed, null) : (reader.Value, ERR.STRING_ISNOTAN_UINT.With(reader, reader.Value));
            }
            else if (implementingType == typeof(long))
            {
                return long.TryParse(reader.Value, out var parsed) ? (parsed, null) : (reader.Value, ERR.STRING_ISNOTA_LONG.With(reader, reader.Value));
            }
            else if (implementingType == typeof(decimal))
            {
                return decimal.TryParse(reader.Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var parsed) ? (parsed, null) : (reader.Value, ERR.STRING_ISNOTA_DECIMAL.With(reader, reader.Value));
            }
            else if (implementingType == typeof(double))
            {
                return double.TryParse(reader.Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var parsed) ? (parsed, null) : (reader.Value, ERR.STRING_ISNOTA_DOUBLE.With(reader, reader.Value));
            }
            else if (implementingType == typeof(float))
            {
                return float.TryParse(reader.Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var parsed) ? (parsed, null) : (reader.Value, ERR.STRING_ISNOTA_FLOAT.With(reader, reader.Value));
            }
            else if (implementingType == typeof(ulong))
            {
                return ulong.TryParse(reader.Value, out var parsed) ? (parsed, null) : (reader.Value, ERR.STRING_ISNOTAN_ULONG.With(reader, reader.Value));
            }
            else if (implementingType.IsEnum)
            {
                return new(reader.Value, null);
            }
            else
            {
                return new(reader.Value, null); //When does this happen? Should we throw an error?
            }


            static (object, ERR?) getByteArrayValue(XmlReader reader)
            {
                try
                {
                    return (Convert.FromBase64String(reader.Value), null);
                }
                catch (FormatException)
                {
                    return (reader.Value, ERR.INCORRECT_BASE64_DATA.With(reader));
                }
            }
        }

        private bool shouldSkipNodeType(XmlNodeType nodeType)
        {
            return nodeType == XmlNodeType.Comment
                || nodeType == XmlNodeType.XmlDeclaration
                || nodeType == XmlNodeType.Whitespace
                || nodeType == XmlNodeType.SignificantWhitespace
                || nodeType == XmlNodeType.CDATA
                || nodeType == XmlNodeType.Notation
                || nodeType == XmlNodeType.ProcessingInstruction;
        }


        /// <summary>
        /// Returns the <see cref="ClassMapping" /> for the object to be deserialized using the root property.
        /// </summary>
        /// <remarks>Assumes the reader is on the start of an object.</remarks>
        internal static (ClassMapping?, FhirXmlException?) DetermineClassMappingFromInstance(XmlReader reader, ModelInspector inspector)
        {
            var (resourceType, error) = determineResourceType(reader);

            if (resourceType is not null)
            {
                var resourceMapping = inspector.FindClassMapping(resourceType);

                return resourceMapping is not null ?
                    (new(resourceMapping, error)) :
                    (new(null, ERR.UNKNOWN_RESOURCE_TYPE.With(reader, resourceType)));
            }
            else
                return new(null, error);
        }

        private static (string, FhirXmlException?) determineResourceType(XmlReader reader)
        {
            return (reader.Name, (reader.NamespaceURI == "http://hl7.org/fhir") ? null : ERR.INCORRECT_ROOT_NAMESPACE);
        }

        /// <summary>
        /// Given a possibly suffixed property name (as encountered in the serialized form), lookup the
        /// mapping for the property and the mapping for the value of the property.
        /// </summary>
        /// <remarks>In case the name is a choice type, the type suffix will be used to determine the returned
        /// <see cref="ClassMapping"/>, otherwise the <see cref="PropertyMapping.ImplementingType"/> is used.
        /// </remarks>
        private static (PropertyMapping? propMapping, ClassMapping? propValueMapping, FhirXmlException? error) tryGetMappedElementMetadata(
            ModelInspector inspector,
            ClassMapping parentMapping,
            XmlReader reader,
            string propertyName)
        {

            var propertyMapping = parentMapping.FindMappedElementByName(propertyName)
                ?? parentMapping.FindMappedElementByChoiceName(propertyName);

            if (propertyMapping is null)
                return (null, null, ERR.UNKNOWN_ELEMENT.With(reader, propertyName));

            (ClassMapping? propertyValueMapping, FhirXmlException? error) = propertyMapping.Choice switch
            {
                ChoiceType.None or ChoiceType.ResourceChoice =>
                    inspector.FindOrImportClassMapping(propertyMapping.ImplementingType) is ClassMapping m
                        ? (m, null)
                        : throw new InvalidOperationException($"Encountered property type {propertyMapping.ImplementingType} for which no mapping was found in the model assemblies. " + reader.GenerateLocationMessage()),
                ChoiceType.DatatypeChoice => getChoiceClassMapping(reader),
                _ => throw new NotImplementedException("Unknown choice type in property mapping. " + reader.GenerateLocationMessage())
            };

            return (propertyMapping, propertyValueMapping, error);

            (ClassMapping?, FhirXmlException?) getChoiceClassMapping(XmlReader r)
            {
                string typeSuffix = propertyName.Substring(propertyMapping.Name.Length);

                return string.IsNullOrEmpty(typeSuffix)
                    ? (null, ERR.CHOICE_ELEMENT_HAS_NO_TYPE.With(r, propertyMapping.Name))
                    : inspector.FindClassMapping(typeSuffix) is ClassMapping cm
                        ? (cm, null)
                        : (default, ERR.CHOICE_ELEMENT_HAS_UNKOWN_TYPE.With(r, propertyMapping.Name, typeSuffix));
            }
        }
    }

    internal class FhirXmlPocoDeserializerState
    {
        public readonly ExceptionAggregator Errors = new();
        public readonly PathStack Path = new();
    }
}

#nullable restore