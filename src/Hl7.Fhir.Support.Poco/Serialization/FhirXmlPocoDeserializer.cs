#nullable enable

using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using System;
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
        /// The kind of object we need to deserialize into, which will influence subtly
        /// how the <see cref="deserializeElementInto{T}(T, ClassMapping, XmlReader, DeserializedObjectKind, FhirXmlPocoDeserializerState)" />
        /// function will operate.
        /// </summary>
        private enum DeserializedObjectKind
        {
            /// <summary>
            /// Deserialize into a complex datatype, and complain about the presence of
            /// a resourceType element.
            /// </summary>
            Complex,

            /// <summary>
            /// Deserialize into a resource
            /// </summary>
            Resource,

            /// <summary>
            /// Deserialize the non-value part of a FhirPrimitive, and do not call validation of
            /// the instance yet, since it will be done when the FhirPrimitive has been constructed
            /// completely, including its value part.
            /// </summary>
            FhirPrimitive
        }

        /// <summary>
        /// Deserialize the FHIR xml from the reader and create a new POCO object containing the data from the reader.
        /// </summary>
        /// <param name="reader">A xml reader positioned on the element of the object, or the beginning of the stream.</param>
        /// <returns>A fully initialized POCO with the data from the reader.</returns>
        public Resource DeserializeResource(XmlReader reader)
        {
            // If the stream has just been opened, move to the first token.
            reader.MoveToContent();

            FhirXmlPocoDeserializerState state = new();

            var result = DeserializeResourceInternal(reader, state);

            return !state.Errors.HasExceptions
                ? result!
                : throw new DeserializationFailedException(result, state.Errors);
        }

        internal Resource? DeserializeResourceInternal(XmlReader reader, FhirXmlPocoDeserializerState state)
        {
            (ClassMapping? resourceMapping, FhirXmlException? error) = DetermineClassMappingFromInstance(reader, _inspector);

            if (resourceMapping is not null)
            {
                // If we have at least a mapping, let's try to continue               
                var newResource = (Base)resourceMapping.Factory();

                try
                {
                    state.Path.EnterResource(resourceMapping.Name);
                    deserializeElementInto(newResource, resourceMapping, reader, DeserializedObjectKind.Resource, state);


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

        private void deserializeElementInto<T>(T target, ClassMapping mapping, XmlReader reader, DeserializedObjectKind kind, FhirXmlPocoDeserializerState state) where T : Base
        {
            while (shouldSkipNodeType(reader.NodeType))
                if (!reader.Read())
                    return;

            //read the next object
            while (reader.Read())
            {
                var name = reader.Name;
                var (propMapping, propValueMapping, error) = tryGetMappedElementMetadata(_inspector, mapping, reader, name);

                // move into first attribute
                while (reader.MoveToNextAttribute())
                {

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
        /// Returns the <see cref="ClassMapping" /> for the object to be deserialized using the `resourceType` property.
        /// </summary>
        /// <remarks>Assumes the reader is on the start of an object.</remarks>
        internal static (ClassMapping?, FhirXmlException?) DetermineClassMappingFromInstance(XmlReader reader, ModelInspector inspector)
        {
            var (resourceType, error) = determineResourceType(reader);

            if (resourceType is not null)
            {
                var resourceMapping = inspector.FindClassMapping(resourceType);

                return resourceMapping is not null ?
                    (new(resourceMapping, null)) :
                    (new(null, ERR.UNKNOWN_RESOURCE_TYPE.With(reader, resourceType)));
            }
            else
                return new(null, error);
        }

        private static (string, FhirXmlException?) determineResourceType(XmlReader reader)
        {
            return (reader.Name, (reader.NamespaceURI != "http://hl7.org/fhir") ? null : ERR.INCORRECT_ROOT_NAMESPACE);
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