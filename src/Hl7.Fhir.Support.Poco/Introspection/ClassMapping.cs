/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Model;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Hl7.Fhir.Introspection
{
    public class ClassMapping : IStructureDefinitionSummary
    {
        private ClassMapping()
        {
            // Force use of TryCreate for users
        }

        /// <summary>
        /// Name of the FHIR datatype/resource this class represents
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The .NET class that implements the FHIR datatype/resource
        /// </summary>
        public Type NativeType { get; private set; }

        /// <summary>
        /// Is True when this class represents a Resource datatype and False if it 
        /// represents a normal complex or primitive Fhir Datatype
        /// </summary>
        public bool IsResource { get; private set; }

        public bool IsCodeOfT { get; private set; }


        private class MappingCollection
        {
            public MappingCollection(Dictionary<string, PropertyMapping> byName, List<PropertyMapping> byOrder)
            {
                ByOrder = byOrder;
                ByName = byName;
            }

            /// <summary>
            /// List of the properties, in the order of appearance.
            /// </summary>
            public readonly List<PropertyMapping> ByOrder;

            /// <summary>
            /// List of the properties, keyed by uppercase name.
            /// </summary>
            public readonly Dictionary<string, PropertyMapping> ByName;
        }

        // This list is created lazily. This not only improves initial startup time of 
        // applications but also ensures circular references between types will not cause loops.
        private MappingCollection Mappings
        {
            get
            {
                LazyInitializer.EnsureInitialized(ref _mappings, _mappingInitializer);
                return _mappings;
            }
        }

        private MappingCollection _mappings;
        private Func<MappingCollection> _mappingInitializer;

        /// <summary>
        /// Secondary index of the PropertyMappings by uppercase name.
        /// </summary>


        public IList<PropertyMapping> PropertyMappings => Mappings.ByOrder;

        /// <summary>
        /// Indicates whether this class represents the nested complex type for a (backbone) element.
        /// </summary>
        public bool IsNestedType { get; private set; }

        string IStructureDefinitionSummary.TypeName =>
            !IsNestedType ? 
                Name
                : NativeType.CanBeTreatedAsType(typeof(BackboneElement)) ?
                    "BackboneElement" 
                    : "Element";

        bool IStructureDefinitionSummary.IsAbstract => NativeType.GetTypeInfo().IsAbstract;

        bool IStructureDefinitionSummary.IsResource => IsResource;

        IReadOnlyCollection<IElementDefinitionSummary> IStructureDefinitionSummary.GetElements() =>
            PropertyMappings.ToReadOnlyCollection();

        /// <summary>
        /// Returns the mapping for an element of this class.
        /// </summary>
        public PropertyMapping FindMappedElementByName(string name)
        {
            if (name == null) throw Error.ArgumentNull(nameof(name));

            var key = name.ToUpperInvariant();
            return Mappings.ByName.TryGetValue(key, out var mapping) ? mapping : null;
        }


        private static T getAttribute<T>(Type t, int version) where T : Attribute
        {
            return ReflectionHelper.GetAttributes<T>(t.GetTypeInfo()).LastOrDefault(isRelevant);

            bool isRelevant(Attribute a) => a is IFhirVersionDependent vd ? vd.AppliesToVersion(version) : true;
        }

        public static bool TryCreate(Type type, out ClassMapping result, int fhirVersion = int.MaxValue)
        {
            result = null;

            var typeAttribute = getAttribute<FhirTypeAttribute>(type, fhirVersion);
            if (typeAttribute == null) return false;

            if (ReflectionHelper.IsOpenGenericTypeDefinition(type))
            {
                Message.Info("Type {0} is marked as a FhirType and is an open generic type, which cannot be used directly to represent a FHIR datatype", type.Name);
                return false;
            }

            result = new ClassMapping
            {
                Name = collectTypeName(typeAttribute, type),
                IsResource = type.CanBeTreatedAsType(typeof(Resource)),
                IsCodeOfT = ReflectionHelper.IsClosedGenericType(type) &&
                                ReflectionHelper.IsConstructedFromGenericTypeDefinition(type, typeof(Code<>)),
                NativeType = type,
                IsNestedType = typeAttribute.IsNestedType,
                _mappingInitializer = () => inspectProperties(type, fhirVersion)
            };

            return true;
        }


        [Obsolete("Create is obsolete, call TryCreate instead, passing in a fhirVersion")]
        public static ClassMapping Create(Type type)
        {
            if (TryCreate(type, out var result))
                return result;

            throw Error.Argument($"Type {nameof(type)} is not marked with the FhirTypeAttribute or is an open generic type");
        }

        /// <summary>
        /// Enumerate this class' properties using reflection, create PropertyMappings
        /// for them and add them to the PropertyMappings.
        /// </summary>
        private static MappingCollection inspectProperties(Type nativeType, int fhirVersion)
        {
            var byName = new Dictionary<string, PropertyMapping>();

            foreach (var property in ReflectionHelper.FindPublicProperties(nativeType))
            {
                if (!PropertyMapping.TryCreate(property, out var propMapping, fhirVersion)) continue;

                var propKey = propMapping.Name.ToUpperInvariant();

                if (byName.ContainsKey(propKey))
                    throw Error.InvalidOperation($"Class has multiple properties that are named '{propKey}'. The property name must be unique");

                byName.Add(propKey, propMapping);
            }

            var ordered = byName.Values.OrderBy(pm => pm.Order).ToList();
            return new MappingCollection(byName, ordered);
        }

        private static string collectTypeName(FhirTypeAttribute attr, Type type)
        {
            var name = attr.Name;

            if (ReflectionHelper.IsClosedGenericType(type))
            {
                name += "<";
                name += String.Join(",", type.GetTypeInfo().GenericTypeArguments.Select(arg => arg.FullName));
                name += ">";
            }

            return name;
        }

        [Obsolete("ClassMapping.IsMappable() is slow and obsolete, use ClassMapping.TryCreate() instead.")]
        public static bool IsMappableType(Type type) => TryCreate(type, out var _);
        
    }
}
