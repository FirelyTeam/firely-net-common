/* 
 * Copyright (c) 2018, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

using Hl7.Fhir.Language;
using Hl7.Fhir.Model.Primitives;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.ElementModel
{
    internal class TypedElementOnSourceNode : ITypedElement, IAnnotated, IExceptionSource, IShortPathGenerator
    {
        public TypedElementOnSourceNode(ISourceNode source, string type, IStructureDefinitionSummaryProvider provider, TypedElementSettings settings = null)
        {
            if (source == null) throw Error.ArgumentNull(nameof(source));

            Provider = provider ?? throw Error.ArgumentNull(nameof(provider));
            _settings = settings ?? new TypedElementSettings();

            if (source is IExceptionSource ies && ies.ExceptionHandler == null)
                ies.ExceptionHandler = (o, a) => ExceptionHandler.NotifyOrThrow(o, a);

            ShortPath = source.Name;
            Source = source;
            (InstanceType, Definition) = buildRootPosition(type);
        }

        private (string instanceType, IElementDefinitionSummary definition) buildRootPosition(string type)
        {
            var rootType = type ?? Source.GetResourceTypeIndicator();
            if (rootType == null)
            {
                if (_settings.ErrorMode == TypedElementSettings.TypeErrorMode.Report)
                    throw Error.Format(nameof(type), $"Cannot determine the type of the root element at '{Source.Location}', " +
                        $"please supply a type argument.");
                else
                    return (rootType, null);
            }

            var elementType = Provider.Provide(rootType);

            if (elementType == null)
            {
                if (_settings.ErrorMode == TypedElementSettings.TypeErrorMode.Report)
                    throw Error.Format(nameof(type), $"Cannot locate type information for type '{rootType}'");
                else
                    return (rootType, null);
            }

            if (elementType.IsAbstract)
                throw Error.Argument(nameof(elementType), $"The type of a node must be a concrete type, '{elementType.TypeName}' is abstract.");

            var rootTypeDefinition = ElementDefinitionSummary.ForRoot(elementType, Source.Name);
            return (rootType, rootTypeDefinition);
        }


        private TypedElementOnSourceNode(TypedElementOnSourceNode parent, ISourceNode source, IElementDefinitionSummary definition, string instanceType, string prettyPath)
        {
            Source = source;
            ShortPath = prettyPath;
            Provider = parent.Provider;
            ExceptionHandler = parent.ExceptionHandler;
            Definition = definition;
            InstanceType = instanceType;
            _settings = parent._settings;
        }

        public ExceptionNotificationHandler ExceptionHandler { get; set; }

        private void raiseTypeError(string message, object source, bool warning = false, string location = null)
        {
            var exMessage = $"Type checking the data: {message}";
            if (!string.IsNullOrEmpty(location))
                exMessage += $" (at {location})";

            var exc = new StructuralTypeException(exMessage);
            var notification = warning ?
                ExceptionNotification.Warning(exc) :
                ExceptionNotification.Error(exc);

            ExceptionHandler.NotifyOrThrow(source, notification);
        }

        public string InstanceType { get; private set; }

        private readonly ISourceNode Source;

        public readonly IStructureDefinitionSummaryProvider Provider;

        private readonly TypedElementSettings _settings;

        public IElementDefinitionSummary Definition { get; private set; }

        public string Name => Definition?.ElementName ?? Source.Name;

        // [EK, 20190822] This is a temporary fix - it brings information in the ElementModel about which
        // FHIR type uses which "universal" primitive type, so a mapping from FHIR.* -> System.*
        // This knowledge is probably needed elsewhere too, and conversely, ElementMode should
        // not be so tightly bound to FHIR here.  If we are going to support V2 or other models,
        // we'd need the same mapping for V2.* -> System.*, so this should actually be pluggable.
        // Maybe get this from the IStructureDefinitionSummaryProvider?  That machine knows what
        // model it is dealing with, so should have knowledge of these mappings.
        // The same holds true for our documentation in the header of ITypedElement, this shows
        // only these mappings, but it should not be bound to a single model.
        //
        // In fact one could derive this dynamically from the structure definition, by looking
        // at the type of the "value" element (e.g. String.value). Although this differs in 
        // R3 and R4, these value (and url and id elements by the way) will indicate which type
        // of "universal" primitive there are, implicitly specifying the mapping between primitive
        // FHIR types and primitive System types.
        private static TypeSpecifier tryMapFhirPrimitiveTypeToSystemType(string fhirType)
        {
            switch (fhirType)
            {
                case "boolean":
                    return TypeSpecifier.Boolean;
                case "integer":
                case "unsignedInt":
                case "positiveInt":
                    return TypeSpecifier.Integer;
                case "time":
                    return TypeSpecifier.Time;
                case "date":
                    return TypeSpecifier.Date;
                case "instant":
                case "dateTime":
                    return TypeSpecifier.DateTime;
                case "decimal":
                    return TypeSpecifier.Decimal;
                case "string":
                case "code":
                case "id":
                case "uri":
                case "oid":
                case "uuid":
                case "canonical":
                case "url":
                case "markdown":
                case "base64Binary":
                case "xhtml":
                    return TypeSpecifier.String;
                default:
                    return null;
            }
        }

        public object Value
        {
            get
            {
                string sourceText = Source.Text;

                if (sourceText == null) return null;

                // If we don't have type information (no definition was found
                // for current node), all we can do is return the underlying string value
                if (InstanceType == null) return sourceText;

                var ts = tryMapFhirPrimitiveTypeToSystemType(InstanceType);
                if (ts == null)
                {
                    raiseTypeError($"Since type {InstanceType} is not a primitive, it cannot have a value", Source, location: Source.Location);
                    return null;
                }

                // Finally, we have a (potentially) unparsed string + type info
                // parse this primitive into the desired type
                if (Any.TryParse(sourceText, ts, out var val))
                    return val;
                else
                {
                    raiseTypeError($"Literal '{sourceText}' cannot be parsed as a {InstanceType}.", Source, location: Source.Location);
                    return sourceText;
                }
            }
        }

        private string deriveInstanceType(ISourceNode current, IElementDefinitionSummary info)
        {
            string instanceType = null;
            var resourceTypeIndicator = current.GetResourceTypeIndicator();

            if (info.IsResource)
            {
                instanceType = resourceTypeIndicator;
                if (instanceType == null) raiseTypeError($"Element '{current.Name}' should contain a resource, but does not actually contain one", current, location: current.Location);
            }
            else if (!info.IsResource && resourceTypeIndicator != null)
            {
                raiseTypeError($"Element '{current.Name}' is not a contained resource, but seems to contain a resource of type '{resourceTypeIndicator}'.", current, location: current.Location);
                instanceType = resourceTypeIndicator;
            }
            else if (info.IsChoiceElement)
            {
                var suffix = current.Name.Substring(info.ElementName.Length);

                if (string.IsNullOrEmpty(suffix))
                {
                    raiseTypeError($"Choice element '{current.Name}' is not suffixed with a type.", current, location: current.Location);
                    instanceType = null;
                }
                else
                {
                    instanceType = info.Type
                        .OfType<IStructureDefinitionReference>()
                        .Select(t => t.ReferredType)
                        .FirstOrDefault(t => string.Compare(t, suffix, StringComparison.OrdinalIgnoreCase) == 0);

                    if (string.IsNullOrEmpty(instanceType))
                        raiseTypeError($"Choice element '{current.Name}' is suffixed with unexpected type '{suffix}'", current, location: current.Location);
                }
            }
            else
            {
                var tp = info.Type.Single();
                instanceType = tp.GetTypeName();
            }

            return instanceType;
        }

        private bool tryGetBySuffixedName(Dictionary<string, IElementDefinitionSummary> dis, string name, out IElementDefinitionSummary info)
        {
            // Simplest case, one on one match between name and element name
            if (dis.TryGetValue(name, out info))
                return true;

            // Now, check the choice elements for a match
            // (this should actually be the longest match, but that's kind of expensive,
            // so as long as we don't add stupid ambiguous choices to a single type, this will work.
            info = dis.Where(kvp => name.StartsWith(kvp.Key) && kvp.Value.IsChoiceElement)
                .Select(kvp => kvp.Value).FirstOrDefault();

            return info != null;
        }

        private IEnumerable<TypedElementOnSourceNode> enumerateElements(Dictionary<string, IElementDefinitionSummary> dis, ISourceNode parent, string name)
        {
            IEnumerable<ISourceNode> childSet;

            // no name filter: work on all the parent's children
            if (name == null)
                childSet = parent.Children();
            else
            {
                var hit = tryGetBySuffixedName(dis, name, out var info);
                childSet = hit && info.IsChoiceElement ?
                    parent.Children(name + "*") :
                    parent.Children(name);
            }

            string lastName = null;
            int _nameIndex = 0;

            foreach (var scan in childSet)
            {
                var hit = tryGetBySuffixedName(dis, scan.Name, out var info);
                string instanceType = info == null ? null :
                    deriveInstanceType(scan, info);

                // If we have definitions for the children, but we didn't find definitions for this 
                // child in the instance, complain
                if (dis.Any() && info == null)
                {
                    raiseTypeError($"Encountered unknown element '{scan.Name}' at location '{scan.Location}' while parsing", this,
                              warning: _settings.ErrorMode != TypedElementSettings.TypeErrorMode.Report);

                    // don't include member, unless we are explicitly told to let it pass
                    if (_settings.ErrorMode != TypedElementSettings.TypeErrorMode.Passthrough)
                        continue;
                }

                if (lastName == scan.Name)
                    _nameIndex += 1;
                else
                {
                    _nameIndex = 0;
                    lastName = scan.Name;
                }

                var prettyPath =
                 hit && !info.IsCollection ? $"{ShortPath}.{info.ElementName}" : $"{ShortPath}.{scan.Name}[{_nameIndex}]";

                yield return new TypedElementOnSourceNode(this, scan, info, instanceType, prettyPath);
            }
        }

        public IEnumerable<ITypedElement> Children(string name = null)
        {
            var childElementDefs = this.ChildDefinitions(Provider).ToDictionary(c => c.ElementName);

            if (Definition != null && !childElementDefs.Any())
            {
                // No type information available for the type representing the children....

                if (InstanceType != null && _settings.ErrorMode == TypedElementSettings.TypeErrorMode.Report)
                    raiseTypeError($"Encountered unknown type '{InstanceType}'", Source, location: Source.Location);

                // Don't go on with the (untyped) children, unless explicitly told to do so
                if (_settings.ErrorMode != TypedElementSettings.TypeErrorMode.Passthrough)
                    return Enumerable.Empty<ITypedElement>();
                else
                    // Ok, pass through the untyped members, but since there is no type information, 
                    // don't bother to run the additional rules
                    return enumerateElements(childElementDefs, Source, name);
            }
            else
                return runAdditionalRules(enumerateElements(childElementDefs, Source, name));
        }

        private IEnumerable<ITypedElement> runAdditionalRules(IEnumerable<ITypedElement> children)
        {
#pragma warning disable 612, 618
            var additionalRules = Source.Annotations(typeof(AdditionalStructuralRule));
            var stateBag = new Dictionary<AdditionalStructuralRule, object>();
            foreach (var child in children)
            {
                foreach (var rule in additionalRules.Cast<AdditionalStructuralRule>())
                {
                    stateBag.TryGetValue(rule, out object state);
                    state = rule(child, this, state);
                    if (state != null) stateBag[rule] = state;
                }

                yield return child;
            }
#pragma warning restore 612, 618
        }

        public string Location => Source.Location;

        public string ShortPath { get; private set; }

        public override string ToString() =>
            $"{(InstanceType != null ? ($"[{InstanceType}] ") : "")}{Source.ToString()}";

        public IEnumerable<object> Annotations(Type type)
        {
#pragma warning disable IDE0046 // Convert to conditional expression
            if (type == typeof(TypedElementOnSourceNode) || type == typeof(ITypedElement) || type == typeof(IShortPathGenerator))
#pragma warning restore IDE0046 // Convert to conditional expression
                return new[] { this };
            else
                return Source.Annotations(type);
        }
    }

    [Obsolete("This class is used for internal purposes and is subject to change without notice. Don't use.")]
    public delegate object AdditionalStructuralRule(ITypedElement node, IExceptionSource ies, object state);
}
