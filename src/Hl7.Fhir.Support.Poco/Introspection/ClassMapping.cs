/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

#nullable enable

using Hl7.Fhir.Model;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using P = Hl7.Fhir.ElementModel.Types;

namespace Hl7.Fhir.Introspection
{
    public class ClassMapping : IStructureDefinitionSummary
    {
        private static readonly ConcurrentDictionary<(Type, FhirRelease), ClassMapping?> _mappedClasses = new();

        public static bool TryGetMappingForType(Type t, FhirRelease release, out ClassMapping? mapping)
        {
            mapping = _mappedClasses.GetOrAdd((t, release), createMapping);
            return mapping is not null;

            static ClassMapping? createMapping((Type, FhirRelease) typeAndRelease) =>
                TryCreate(typeAndRelease.Item1, out var m, typeAndRelease.Item2) ? m : null;
        }

        public static void AddMappingForType(Type t, FhirRelease release, ClassMapping mapping)
        {
            _mappedClasses[(t, release)] = mapping;
        }

        private ClassMapping(string name, Type nativeType)
        {
            Name = name;
            NativeType = nativeType;
            _mappingInitializer = () => new PropertyMappingCollection();
        }

        /// <summary>
        /// Name of the mapping.
        /// </summary>
        /// <remarks>
        /// This is often the FHIR name for the type, but not always:
        /// <list type="bullet">
        /// <item>FHIR <c>code</c> types with required bindings are modelled in the POCO as a <see cref="Code{T}"/>,
        /// the mapping name for these will be <c>code&lt;name of enum&gt;</c></item>
        /// <item>Nested (backbone)types have a mapping name that includes the full path to the element defining
        /// the nested type, e.g. <c>Patient#Patient.contact</c></item>
        /// </list>
        /// </remarks>
        public string Name { get; private set; }

        /// <summary>
        /// The .NET class that implements the FHIR datatype/resource
        /// </summary>
        public Type NativeType { get; private set; }

        [Obsolete("This property is never initialized and its value will always be null.")]
        public Type? DeclaredType { get; private set; } = null;

        /// <summary>
        /// Is <c>true</c> when this class represents a Resource datatype.
        /// </summary>
        public bool IsResource { get; private set; } = false;

        /// <summary>
        /// Is <c>true</c> when this class represents a code with a required binding.
        /// </summary>
        /// <remarks>See <see cref="Name"></see>.</remarks>
        public bool IsCodeOfT { get; private set; } = false;

        /// <summary>
        /// Indicates whether this class represents the nested complex type for a (backbone) element.
        /// </summary>
        public bool IsNestedType { get; private set; } = false;

        /// <summary>
        /// The canonical for the StructureDefinition defining this type
        /// </summary>
        /// <remarks>Will be null for backbone types.</remarks>
        public string? Canonical { get; private set; }

        private class PropertyMappingCollection
        {
            public PropertyMappingCollection()
            {
                // nothing deyond default initializers.
            }

            public PropertyMappingCollection(Dictionary<string, PropertyMapping> byName, List<PropertyMapping> byOrder)
            {
                ByOrder = byOrder;
                ByName = byName;

                if (byName.Comparer != StringComparer.OrdinalIgnoreCase)
                    throw new ArgumentException("Dictionary should be keyed by OrdinalIgnoreCase.");
            }

            /// <summary>
            /// List of the properties, in the order of appearance.
            /// </summary>
            public readonly List<PropertyMapping> ByOrder = new();

            /// <summary>
            /// List of the properties, keyed by name.
            /// </summary>
            public readonly Dictionary<string, PropertyMapping> ByName = new();
        }

        // This list is created lazily. This not only improves initial startup time of 
        // applications but also ensures circular references between types will not cause loops.
        private PropertyMappingCollection Mappings
        {
            get
            {
                LazyInitializer.EnsureInitialized(ref _mappings, _mappingInitializer);
                return _mappings!;
            }
        }

        private PropertyMappingCollection? _mappings;
        private Func<PropertyMappingCollection> _mappingInitializer;

        /// <summary>
        /// Secondary index of the PropertyMappings by uppercase name.
        /// </summary>


        public IReadOnlyList<PropertyMapping> PropertyMappings => Mappings.ByOrder;

        /// <summary>
        /// Holds a reference to a property that represents a primitive FHIR value. This
        /// property will also be present in the PropertyMappings collection. If this class has 
        /// no such property, it is null. 
        /// </summary>
        public PropertyMapping PrimitiveValueProperty => PropertyMappings.SingleOrDefault(pm => pm.RepresentsValueElement);

        public bool HasPrimitiveValueMember => PropertyMappings.Any(pm => pm.RepresentsValueElement);

        string IStructureDefinitionSummary.TypeName =>
            this switch
            {
                { IsCodeOfT: true } => "code",
                { IsNestedType: true } => NativeType.CanBeTreatedAsType(typeof(BackboneElement)) ?
                            "BackboneElement"
                            : "Element",
                _ => Name
            };

        bool IStructureDefinitionSummary.IsAbstract =>
            ((IStructureDefinitionSummary)this).TypeName == "BackboneElement" || NativeType.GetTypeInfo().IsAbstract;

        bool IStructureDefinitionSummary.IsResource => IsResource;

        IReadOnlyCollection<IElementDefinitionSummary> IStructureDefinitionSummary.GetElements() =>
            PropertyMappings.Where(pm => !pm.RepresentsValueElement).ToList();

        /// <summary>
        /// Returns the mapping for an element of this class.
        /// </summary>
        public PropertyMapping? FindMappedElementByName(string name)
        {
            if (name == null) throw Error.ArgumentNull(nameof(name));

            return Mappings.ByName.TryGetValue(name, out var mapping) ? mapping : null;
        }

        /// <summary>
        /// Returns the mapping for an element of this class by a name that
        /// might be suffixed by a type name (e.g. for choice elements).
        /// </summary>
        public PropertyMapping? FindMappedElementByChoiceName(string name)
        {
            if (name == null) throw Error.ArgumentNull(nameof(name));

            // Now, check the choice elements for a match
            // (this should actually be the longest match, but that's kind of expensive,
            // so as long as we don't add stupid ambiguous choices to a single type, this will work.
            return Mappings.ByOrder
                .Where(m => name.StartsWith(m.Name) && m.Choice == ChoiceType.DatatypeChoice)
                .FirstOrDefault();
        }

        internal static T? GetAttribute<T>(MemberInfo t, FhirRelease version) where T : Attribute
        {
            return ReflectionHelper.GetAttributes<T>(t).LastOrDefault(isRelevant);

            bool isRelevant(Attribute a) => a is not IFhirVersionDependent vd || a.AppliesToRelease(version);
        }

        public static bool TryCreate(Type type, out ClassMapping? result, FhirRelease fhirVersion = (FhirRelease)int.MaxValue)
        {
            // Simulate reading the ClassMappings from the primitive types (from http://hl7.org/fhirpath namespace).
            // These are in fact defined as POCOs in Hl7.Fhir.ElementModel.Types,
            // but we cannot reflect on them, mainly because the current organization of our assemblies and
            // namespaces make it impossible to include them under Introspection. This is not a showstopper,
            // since these basic primitives have hardly any additional metadata apart from their names.            
            result = CqlPrimitiveTypes.SingleOrDefault(m => m.NativeType == type);
            if (result is not null) return true;

            var typeAttribute = GetAttribute<FhirTypeAttribute>(type.GetTypeInfo(), fhirVersion);
            if (typeAttribute == null) return false;

            if (ReflectionHelper.IsOpenGenericTypeDefinition(type))
            {
                Message.Info("Type {0} is marked as a FhirType and is an open generic type, which cannot be used directly to represent a FHIR datatype", type.Name);
                return false;
            }

            result = new ClassMapping(collectTypeName(typeAttribute, type), type)
            {
                IsResource = typeAttribute.IsResource || type.CanBeTreatedAsType(typeof(Resource)),
                IsCodeOfT = ReflectionHelper.IsClosedGenericType(type) &&
                                ReflectionHelper.IsConstructedFromGenericTypeDefinition(type, typeof(Code<>)),
                IsNestedType = typeAttribute.IsNestedType,
                _mappingInitializer = () => inspectProperties(type, fhirVersion),
                Canonical = typeAttribute.Canonical
            };

            return true;
        }


        [Obsolete("Create is obsolete, call TryCreate instead, passing in a fhirVersion")]
        public static ClassMapping Create(Type type)
        {
            if (TryCreate(type, out var result))
                return result!;

            throw Error.Argument($"Type {nameof(type)} is not marked with the FhirTypeAttribute or is an open generic type");
        }

        /// <summary>
        /// Enumerate this class' properties using reflection, create PropertyMappings
        /// for them and add them to the PropertyMappings.
        /// </summary>
        private static PropertyMappingCollection inspectProperties(Type nativeType, FhirRelease fhirVersion)
        {
            var byName = new Dictionary<string, PropertyMapping>(StringComparer.OrdinalIgnoreCase);

            foreach (var property in ReflectionHelper.FindPublicProperties(nativeType))
            {
                if (!PropertyMapping.TryCreate(property, out var propMapping, fhirVersion)) continue;

                var propKey = propMapping!.Name;

                if (byName.ContainsKey(propKey))
                    throw Error.InvalidOperation($"Class has multiple properties that are named '{propKey}'. The property name must be unique");

                byName.Add(propKey, propMapping);
            }

            var ordered = byName.Values.OrderBy(pm => pm.Order).ToList();
            return new PropertyMappingCollection(byName, ordered);
        }

        private static string collectTypeName(FhirTypeAttribute attr, Type type)
        {
            var name = attr.Name;

            if (ReflectionHelper.IsClosedGenericType(type))
            {
                name += "<";
                name += string.Join(",", type.GetTypeInfo().GenericTypeArguments.Select(arg => arg.FullName));
                name += ">";
            }

            return name;
        }

        [Obsolete("ClassMapping.IsMappable() is slow and obsolete, use ClassMapping.TryCreate() instead.")]
        public static bool IsMappableType(Type type) => TryCreate(type, out var _);


        internal static ClassMapping[] CqlPrimitiveTypes = new[]
        {
            new ClassMapping("System.Boolean", typeof(P.Boolean)),
            new ClassMapping("System.String", typeof(P.String)),
            new ClassMapping("System.Integer", typeof(P.Integer)),
            new ClassMapping("System.Decimal", typeof(P.Decimal)),
            new ClassMapping("System.DateTime", typeof(P.DateTime)),
            new ClassMapping("System.Time", typeof(P.Time)),
            new ClassMapping("System.Quantity", typeof(P.Quantity))
        };
    }
}

#nullable restore