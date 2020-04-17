/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hl7.Fhir.Introspection
{
    public class ClassMapping
    {
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

        /// <summary>
        /// PropertyMappings indexed by uppercase name for access speed
        /// </summary>
        private readonly Dictionary<string, PropertyMapping> _propMappings = new Dictionary<string, PropertyMapping>();

        /// <summary>
        /// Collection of PropertyMappings that capture information about this classes
        /// properties
        /// </summary>
        public IList<PropertyMapping> PropertyMappings { get; private set; }

        /// <summary>
        /// Holds a reference to a property that represents a primitive FHIR value. This
        /// property will also be present in the PropertyMappings collection. If this class has 
        /// no such property, it is null. 
        /// </summary>
        public PropertyMapping PrimitiveValueProperty { get; private set; }

        public bool HasPrimitiveValueMember => PrimitiveValueProperty != null;

        /// <summary>
        /// Returns the mapping for an element of this class.
        /// </summary>
        /// <param name="name">The name of the element, may include the type suffix for choice elements.</param>
        /// <returns></returns>
        public PropertyMapping FindMappedElementByName(string name)
        {
            if (name == null) throw Error.ArgumentNull(nameof(name));

            var normalizedName = name.ToUpperInvariant();
            if( _propMappings.TryGetValue(normalizedName, out PropertyMapping prop)) return prop;

            // Not found, maybe a polymorphic name
            return PropertyMappings.SingleOrDefault(p => p.MatchesSuffixedName(name));
        }


        private static T getAttribute<T>(Type t, string version) where T : Attribute
        {
            return ReflectionHelper.GetAttributes<T>(t.GetTypeInfo()).LastOrDefault(isRelevant);

            bool isRelevant(Attribute a) => a is IFhirVersionDependent vd ? vd.AppliesToVersion(version) : true;
        }

        public static bool TryCreate(Type type, out ClassMapping result, string fhirVersion = null)
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
            };

            result.inspectProperties(fhirVersion);

            return true;
        }

        [Obsolete("Create is obsolete, call TryCreate instead, passing in a fhirVersion")]
        public static ClassMapping Create(Type type)
        {
            if(TryCreate(type, out var result, fhirVersion:null))
                return result;

            throw Error.Argument($"Type {nameof(type)} is not marked with the FhirTypeAttribute or is an open generic type");
        }

        /// <summary>
        /// Enumerate this class' properties using reflection, create PropertyMappings
        /// for them and add them to the PropertyMappings.
        /// </summary>
        private void inspectProperties(string fhirVersion)
        {
            foreach (var property in ReflectionHelper.FindPublicProperties(NativeType))
            {
                if (!PropertyMapping.TryCreate(property, out var propMapping, fhirVersion)) continue;

                var propKey = propMapping.Name.ToUpperInvariant();

                if (_propMappings.ContainsKey(propKey))
                    throw Error.InvalidOperation($"Class has multiple properties that are named '{propKey}'. The property name must be unique");

                _propMappings.Add(propKey, propMapping);

                // Keep a pointer to this property if this is a primitive value element ("Value" in primitive types)
                if (propMapping.RepresentsValueElement)
                    PrimitiveValueProperty = propMapping;
            }

            PropertyMappings = _propMappings.Values.OrderBy(prop => prop.Order).ToList();
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
        public static bool IsMappableType(Type type) => TryCreate(type, out var _, fhirVersion: null);
    }

    public static class IntrospectionTypeExtensions
    {
        /// <summary>
        /// Determines whether the given type is a POCO type representing a complex substructure in a Resource or datatype.
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        /// <remarks>These are sometimes called <c>BackboneElements</c>, but may actually also be <c>Elements</c>,
        /// dependent on whether <c>modifier extensions</c> are allowed at that point.</remarks>
        public static bool RepresentsComplexElementType(this Type me) => !(me.CanBeTreatedAsType(typeof(DataType)))
            && (me.CanBeTreatedAsType(typeof(Element)) || me.CanBeTreatedAsType(typeof(BackboneElement)))
            && (me != typeof(Element)) && (me != typeof(BackboneElement));
    }

}
