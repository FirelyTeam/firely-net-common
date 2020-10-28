/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
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
            // No public constructor.
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

        /// <summary>
        /// 
        /// </summary>
        public Type DeclaredType { get; private set; }

        /// <summary>
        /// Is <c>true</c> when this class represents a Resource datatype.
        /// </summary>
        public bool IsResource { get; private set; }

        /// <summary>
        /// Is <c>true</c> when this class represents a code with a required binding.
        /// </summary>
        /// <remarks>See <see cref="Name"></see>.</remarks>
        public bool IsCodeOfT { get; private set; }

        /// <summary>
        /// Indicates whether this class represents the nested complex type for a (backbone) element.
        /// </summary>
        public bool IsNestedType { get; private set; }

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
        /// Holds a reference to a property that represents a primitive FHIR value. This
        /// property will also be present in the PropertyMappings collection. If this class has 
        /// no such property, it is null. 
        /// </summary>
        public PropertyMapping PrimitiveValueProperty => PropertyMappings.SingleOrDefault(pm => pm.RepresentsValueElement);

        public bool HasPrimitiveValueMember => PropertyMappings.Any(pm => pm.RepresentsValueElement);

        string IStructureDefinitionSummary.TypeName
        {
            get
            {
                if (IsCodeOfT) 
                    return "code";
                else if (IsNestedType)
                {
                    return NativeType.CanBeTreatedAsType(typeof(BackboneElement)) ?
                        "BackboneElement"
                        : "Element";
                }
                else
                    return Name;
            }
        }

        bool IStructureDefinitionSummary.IsAbstract => NativeType.GetTypeInfo().IsAbstract;

        bool IStructureDefinitionSummary.IsResource => IsResource;

        IReadOnlyCollection<IElementDefinitionSummary> IStructureDefinitionSummary.GetElements() =>
            PropertyMappings.Where(pm => !pm.RepresentsValueElement).ToList();

        /// <summary>
        /// Returns the mapping for an element of this class.
        /// </summary>
        public PropertyMapping FindMappedElementByName(string name)
        {
            if (name == null) throw Error.ArgumentNull(nameof(name));

            var key = name.ToUpperInvariant();
            return Mappings.ByName.TryGetValue(key, out var mapping) ? mapping : null;
        }


        private static T getAttribute<T>(Type t, FhirRelease version) where T : Attribute
        {
            return ReflectionHelper.GetAttributes<T>(t.GetTypeInfo()).LastOrDefault(isRelevant);

            bool isRelevant(Attribute a) => a is IFhirVersionDependent vd ? vd.AppliesToVersion(version) : true;
        }

        public static bool TryCreate(Type type, out ClassMapping result, FhirRelease fhirVersion = (FhirRelease)int.MaxValue)
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
                IsResource = typeAttribute.IsResource || type.CanBeTreatedAsType(typeof(Resource)),
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
        private static MappingCollection inspectProperties(Type nativeType, FhirRelease fhirVersion)
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
