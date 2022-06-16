#nullable enable

using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using System;
using System.Collections;
using System.Collections.Generic;
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
            // If the stream has just been opened, move to the first token.

            //TODO: Processing instructions only UTF-8
            reader.MoveToContent();

            FhirXmlPocoDeserializerState state = new();

            var result = DeserializeResourceInternal(reader, state);

            return !state.Errors.HasExceptions
                ? result!
                : throw new DeserializationFailedException(result, state.Errors);
        }

        public Base DeserializeDatatype(Type targetType, XmlReader reader)
        {
            // If the stream has just been opened, move to the first token.

            //TODO: Processing instructions only UTF-8
            reader.MoveToContent();

            FhirXmlPocoDeserializerState state = new();

            var mapping = _inspector.FindOrImportClassMapping(targetType) ??
               throw new ArgumentException($"Type '{targetType}' could not be located in model assembly '{Assembly}' and can " +
                   $"therefore not be used for deserialization. " + reader.GenerateLocationMessage(), nameof(targetType));

            var result = DeserializeDatatypeInternal(mapping, reader, state);

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
            return null;
        }

        internal Base? DeserializeDatatypeInternal(ClassMapping datatypeMapping, XmlReader reader, FhirXmlPocoDeserializerState state)
        {

            if (datatypeMapping is not null)
            {
                // If we have at least a mapping, let's try to continue               
                var newDatatype = (Base)datatypeMapping.Factory();
                deserializeDatatypeInto(newDatatype, datatypeMapping, reader, state);
                return newDatatype;
            }
            return null;
        }

        /// <summary>
        /// We expect to start at the open tag op the element. When done, the reader will be at the next token after this element or end of the file.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="mapping"></param>
        /// <param name="reader"></param>
        /// <param name="state"></param>
        private void deserializeDatatypeInto(Base target, ClassMapping mapping, XmlReader reader, FhirXmlPocoDeserializerState state)
        {
            state.Path.EnterElement(mapping.Name);

            //check if on opening tag
            if (reader.NodeType != XmlNodeType.Element)
            {
                //throw exception, we have made a mistake
                throw new Exception("error, not a start tag");
            }

            if (reader.HasAttributes)
            {
                readAttributes(target, mapping, reader, state);
            }

            //Empty elements have no children e.g. <foo value="bar/>)
            if (!reader.IsEmptyElement)
            {
                reader.Read();
                var encounteredListElements = new List<string>();
                //read the next object
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    // check if we are currently on an element.
                    // check if correct namespace.
                    // TODO XHTML.
                    // TODO contained resources.
                    var name = reader.Name;
                    var (propMapping, propValueMapping, error) = tryGetMappedElementMetadata(_inspector, mapping, reader, name);
                    // contained resource: propMapping.IsResourceChoice

                    if (propMapping is not null)
                    {
                        // check propMapping if list -> readList or readSingle value 
                        if (propMapping.IsCollection == true)
                        {
                            var newCollection = createOrExpandList(target, propValueMapping!, propMapping, reader, encounteredListElements, state);
                            propMapping!.SetValue(target, newCollection);
                        }
                        else
                        {
                            var newProperty = readSingleValue(propValueMapping!, reader, state);
                            propMapping!.SetValue(target, newProperty);
                        }
                    }
                    else
                    {
                        //ERR.UNKNOWN_PROPERTY_FOUND;
                        //TODO: Create extension method to skip current element, and move to next.
                    }
                }
            }
            reader.Read();
            state.Path.ExitElement();
        }

        //Will create a new list, or adds encountered values to an already existing list (and reports a user error).
        private IList? createOrExpandList(Base target, ClassMapping propValueMapping, PropertyMapping propMapping, XmlReader reader, List<string> encounteredListElements, FhirXmlPocoDeserializerState state)
        {
            IList? newCollection;
            //Was there already a list created previously? -> User error! But let's fix it, and expand the list with the newly encountered element(s).
            if (encounteredListElements.Contains(reader.Name))
            {
                //TODO: state.Errors.Add() add extension that element is in the wrong order.

                var currentList = (IList?)propMapping.GetValue(target)!;
                newCollection = (currentList is not null) ? expandCurrentList(currentList, propValueMapping, propMapping, reader, state) : readList(propValueMapping!, reader, state);
            }
            else //Create a new list.
            {
                encounteredListElements.Add(reader.Name);
                newCollection = readList(propValueMapping!, reader, state);
            }
            return newCollection;
        }

        //Retrieves previously created list, and add currently encountered values.
        private IList? expandCurrentList(IList currentEntries, ClassMapping propValueMapping, PropertyMapping propMapping, XmlReader reader, FhirXmlPocoDeserializerState state)
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
            var name = reader.Name;

            while (reader.Name == name)
            {
                var newDatatype = (Base)propValueMapping.Factory();
                deserializeDatatypeInto(newDatatype, propValueMapping, reader, state);
                list.Add(newDatatype);
            }
            return list;
        }

        private Base readSingleValue(ClassMapping propValueMapping, XmlReader reader, FhirXmlPocoDeserializerState state)
        {
            var instance = (Base)propValueMapping.Factory();
            deserializeDatatypeInto(instance, propValueMapping, reader, state);
            return instance;
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
                            //skip namespace
                            //TODO: unknown property. 
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
            var (parsedValue, error) = parseValue(reader, propMapping.ImplementingType);

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
            else
            {
                //TODO: handle error
            }
        }

        private (object?, FhirXmlException?) parseValue(XmlReader reader, Type implementingType)
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
            else if (implementingType == typeof(long))
            {
                return long.TryParse(reader.Value, out var parsed) ? (parsed, null) : (reader.Value, ERR.STRING_ISNOTA_LONG.With(reader, reader.Value));
            }
            else if (implementingType == typeof(decimal))
            {
                return decimal.TryParse(reader.Value, out var parsed) ? (parsed, null) : (reader.Value, ERR.STRING_ISNOTA_DECIMAL.With(reader, reader.Value));
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
                return (null, null, ERR.UNKNOWN_PROPERTY_FOUND.With(reader, propertyName));

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