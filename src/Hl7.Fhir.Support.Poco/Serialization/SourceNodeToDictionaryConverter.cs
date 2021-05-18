#nullable enable

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using P = Hl7.Fhir.ElementModel.Types;

namespace Hl7.Fhir.Serialization
{
    public class SourceNodeToDictionaryConverter
    {
        public static string TYPE_KEY = "_type";
        private readonly Assembly _modelAssembly;

        public SourceNodeToDictionaryConverter(Assembly modelAssembly)
        {
            _modelAssembly = modelAssembly ?? throw new ArgumentNullException(nameof(modelAssembly));
        }

        public IDictionary<string, object> Load(ISourceNode source)
        {
            var rootTypeName = source.GetResourceTypeIndicator();

            if (rootTypeName is not null)
            {
                Type? pocoType = _modelAssembly.GetFhirTypeByName(rootTypeName);

                return pocoType is not null
                    ? Load(source, pocoType)
                    : throw buildTypeError($"Cannot load type information for type '{rootTypeName}'", source.Location);
            }
            else
            {
                throw buildTypeError("Cannot determine the type of the resource.", source.Location);
            }
        }


        public IDictionary<string, object> Load(ISourceNode source, Type elementType)
        {
            //if (elementType.GetTypeInfo().IsAbstract)
            //    throw buildTypeError($"The type of an element must be a concrete type, '{elementType.GetFhirTypeName()}' is abstract.", source.Location);

            IDictionary<string, PropertyInfo> childDefs = elementType.GetFhirProperties();
            IEnumerable<ISourceNode> childSet = source.Children();
            List<string>? unknownElements = null;
            Dictionary<string, object> children = new();

            children.Add(TYPE_KEY, elementType);

            foreach (var scan in childSet)
            {
                var hit = tryGetBySuffixedName(childDefs, scan.Name, out var pi);

                if (!hit)
                {
                    if (unknownElements is null) unknownElements = new();
                    unknownElements.Add(scan.Name);
                    continue;
                }

                //TODO: child with Value
                //TODO: group children in lists

                if (pi.IsContainedResource())
                    children.Add(scan.Name, Load(scan));
                else
                {
                    // this handles both datatype choices and & single type elements
                    var instanceType = deriveInstanceType(scan, pi);
                    children.Add(scan.Name, Load(scan, instanceType));
                }
            }

            if (unknownElements is not null)
                throw buildTypeError($"Encountered unknown child element(s) '{string.Join(", ", unknownElements)}'.",
                    source.Location);

            return children;
        }

        // Derive the instance type 
        private Type deriveInstanceType(ISourceNode current, PropertyInfo pi)
        {
            var resourceTypeIndicator = current.GetResourceTypeIndicator();

            // Instead of this "validation", just include the "resourceType"
            if (resourceTypeIndicator != null)
                throw buildTypeError($"Element '{current.Name}' is not a contained resource, but seems to contain a resource of type '{resourceTypeIndicator}'.", current.Location);

            if (!pi.IsChoiceElement())
                return pi.GetPropertyTypeForElement();
            else
            {
                var elementName = pi.GetElementName();
                var suffix = current.Name.Substring(elementName.Length);

                if (string.IsNullOrEmpty(suffix))
                    throw buildTypeError($"Choice element '{current.Name}' is not suffixed with a type.", current.Location);

                var runtimeType = _modelAssembly.GetFhirTypeByName(suffix, StringComparison.OrdinalIgnoreCase);
                if (runtimeType is null)
                    throw buildTypeError($"Cannot load type information for type '{suffix}'", current.Location);

                //In the philosohpy of not doing validation while parsing,
                //we should not do this check anymore.
                //var allowedTypes = pi.GetAllowedTypes();
                //if (!allowedTypes.Any(t => t.IsAssignableFrom(runtimeType)))
                //    throw buildTypeError($"Choice element '{current.Name}' is suffixed with unexpected type '{suffix}'", current.Location);

                return runtimeType;
            }
        }

        private static StructuralTypeException buildTypeError(string message, string? location = null)
        {
            var exMessage = $"Type checking the data: {message}";
            if (!string.IsNullOrEmpty(location))
                exMessage += $" (at {location})";

            return new StructuralTypeException(exMessage);
        }

        private object? parseValue(ISourceNode node, Type primitiveType)
        {
            if (node.Text is not string text) return null;

            // Finally, we have a (potentially) unparsed string + type info
            // parse this primitive into the desired type
            if (P.Any.TryParse(text, primitiveType, out var val))
                return val;
            else
            {
                //throw buildTypeError($"Literal '{text}' cannot be parsed as a {primitiveType.GetFhirTypeName()}.", node.Location);
                return text;
            }
        }

        private static bool tryGetBySuffixedName(IDictionary<string, PropertyInfo> dis, string name, out PropertyInfo pi)
        {
            // Simplest case, one on one match between name and element name
            if (dis.TryGetValue(name, out pi))
                return true;

            // Now, check the choice elements for a match
            // (this should actually be the longest match, but that's kind of expensive,
            // so as long as we don't add stupid ambiguous choices to a single type, this will work.
            pi = dis.Where(kvp => name.StartsWith(kvp.Key) && kvp.Value.IsChoiceElement())
                .Select(kvp => kvp.Value).FirstOrDefault();

            return pi != null;
        }
    }
}

#nullable restore