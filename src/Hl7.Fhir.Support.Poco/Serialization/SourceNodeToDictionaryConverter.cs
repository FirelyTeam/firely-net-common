﻿#nullable enable

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Introspection;
using System;
using System.Collections.Generic;
using System.Reflection;
using P = Hl7.Fhir.ElementModel.Types;

namespace Hl7.Fhir.Serialization
{
    public class SourceNodeToDictionaryConverter
    {
        public static string TYPE_KEY = "_type";
        private readonly ModelInspector _inspector;

        public SourceNodeToDictionaryConverter(Assembly modelAssembly)
        {
            _inspector = modelAssembly is not null ?
                ModelInspector.ForAssembly(modelAssembly)
                : throw new ArgumentNullException(nameof(modelAssembly));
        }

        public IDictionary<string, object> Load(ISourceNode source)
        {
            var rootTypeName = source.GetResourceTypeIndicator();

            if (rootTypeName is not null)
            {
                ClassMapping? classMapping = _inspector.FindClassMapping(rootTypeName);

                return classMapping is not null
                    ? Load(source, classMapping)
                    : throw buildTypeError($"Cannot load type information for type '{rootTypeName}'", source.Location);
            }
            else
            {
                throw buildTypeError("Cannot determine the type of the resource.", source.Location);
            }
        }

        public IDictionary<string, object> Load(ISourceNode source, Type elementType)
        {
            ClassMapping? classMapping = _inspector.ImportType(elementType);

            return classMapping is not null
                ? Load(source, classMapping)
                : throw buildTypeError($"Cannot load FHIR type information from .NET type '{elementType.Name}'. Is this an existing type tagged with [FhirType] for release {_inspector.FhirRelease}?", source.Location);
        }

        internal IDictionary<string, object> Load(ISourceNode source, ClassMapping classMapping)
        {
            //if (elementType.GetTypeInfo().IsAbstract)
            //    throw buildTypeError($"The type of an element must be a concrete type, '{elementType.GetFhirTypeName()}' is abstract.", source.Location);

            IEnumerable<ISourceNode> childSet = source.Children();
            List<string>? unknownElements = null;
            Dictionary<string, object> children = new();

            children.Add(TYPE_KEY, classMapping.NativeType);

            if (source.Text is not null)
            {
                object value = classMapping.HasPrimitiveValueMember ?
                    parseValue(source.Text, classMapping.PrimitiveValueProperty.ImplementingType)
                   : source.Text;

                children.Add("value", value);
            }

            foreach (var scan in childSet)
            {
                var childPropertyMapping = classMapping.FindMappedElementByName(scan.Name) ??
                        classMapping.FindMappedElementByChoiceName(scan.Name);
                if (childPropertyMapping is null)
                {
                    if (unknownElements is null) unknownElements = new();
                    unknownElements.Add(scan.Name);
                    continue;
                }

                //TODO: group children in lists

                if (childPropertyMapping.Choice == ChoiceType.ResourceChoice)
                    children.Add(scan.Name, Load(scan));
                else
                {
                    if (childPropertyMapping.IsPrimitive && scan.Text is not null)
                        children.Add(scan.Name, parseValue(scan.Text, childPropertyMapping.ImplementingType));
                    else
                    {
                        // this handles both datatype choices and & single type elements
                        var childClassMapping = determineClassMapping(scan, childPropertyMapping);
                        children.Add(scan.Name, Load(scan, childClassMapping));
                    }
                }
            }

            if (unknownElements is not null)
                throw buildTypeError($"Encountered unknown child element(s) '{string.Join(", ", unknownElements)}'.",
                    source.Location);

            return children;
        }

        // Derive the instance type 
        private ClassMapping determineClassMapping(ISourceNode current, PropertyMapping pm)
        {
            if (current.GetResourceTypeIndicator() is var resourceTypeIndicator)
                throw buildTypeError($"Element '{current.Name}' is not a contained resource, but seems to contain a resource of type '{resourceTypeIndicator}'.", current.Location);

            if (pm.Choice == ChoiceType.None)
                return _inspector.FindClassMapping(pm.FhirType[0]);
            else
            {
                var elementName = pm.Name;
                var suffix = current.Name.Substring(elementName.Length);

                if (string.IsNullOrEmpty(suffix))
                    throw buildTypeError($"Choice element '{current.Name}' is not suffixed with a type.", current.Location);

                return _inspector.FindClassMapping(suffix) ??
                    throw buildTypeError($"Cannot load type information for type '{suffix}'", current.Location);

                //    //In the philosohpy of not doing validation while parsing,
                //    //we should not do this check anymore.
                //    //var allowedTypes = pi.GetAllowedTypes();
                //    //if (!allowedTypes.Any(t => t.IsAssignableFrom(runtimeType)))
                //    //    throw buildTypeError($"Choice element '{current.Name}' is suffixed with unexpected type '{suffix}'", current.Location);
            }
        }

        private static StructuralTypeException buildTypeError(string message, string? location = null)
        {
            var exMessage = $"Type checking the data: {message}";
            if (!string.IsNullOrEmpty(location))
                exMessage += $" (at {location})";

            return new StructuralTypeException(exMessage);
        }

        private static object parseValue(string text, Type primitiveType)
        {
            // Finally, we have a (potentially) unparsed string + type info
            // parse this primitive into the desired type
            if (P.Any.TryParse(text, primitiveType, out var val))
                return val!;
            else
            {
                //throw buildTypeError($"Literal '{text}' cannot be parsed as a {primitiveType.GetFhirTypeName()}.", node.Location);
                return text;
            }
        }
    }
}

#nullable restore