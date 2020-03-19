/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Hl7.Fhir.Introspection
{
    [System.Diagnostics.DebuggerDisplay(@"\{Name={Name} ElementType={ElementType.Name}}")]
    public class PropertyMapping
    {
        public string Name { get; internal set; }

        public bool IsCollection { get; internal set; }

        public bool IsPrimitive { get; private set; }
        public bool RepresentsValueElement { get; private set; }
        public bool InSummary { get; private set; }
        public bool IsMandatoryElement { get; private set; }

        public Type ElementType { get; private set; }
        public bool IsBackboneElement { get; private set; }

        public int Order { get; private set; }

        public XmlRepresentation SerializationHint { get; private set; }

        public ChoiceType Choice { get; private set; }
        public Type[] FhirType { get; private set; }        // may be multiple if this is a choice
        public bool IsOpen { get; private set; }


        private static T getAttribute<T>(PropertyInfo p, string version) where T : Attribute
        {
            return ReflectionHelper.GetAttributes<T>(p).LastOrDefault(isRelevant);

            bool isRelevant(Attribute a) => a is IFhirVersionDependent vd ? vd.AppliesToVersion(version) : true;
        }

        public static PropertyMapping Create(PropertyInfo prop, string version=null) => Create(prop, out var _, version);
        
        internal static PropertyMapping Create(PropertyInfo prop, out IEnumerable<Type> referredTypes, string version)
        {
            if (prop == null) throw Error.ArgumentNull(nameof(prop));

            var foundTypes = new List<Type>();

            PropertyMapping result = new PropertyMapping();

            var elementAttr = getAttribute<FhirElementAttribute>(prop, version);
            result.Name = determinePropertyName(elementAttr,prop);
            result.ElementType = prop.PropertyType;

            result.InSummary = elementAttr?.InSummary ?? false;
            
            var cardinalityAttr = getAttribute<Validation.CardinalityAttribute>(prop, version);
            result.IsMandatoryElement = cardinalityAttr != null ? cardinalityAttr.Min > 0 : false;
            
            result.Choice = elementAttr?.Choice ?? ChoiceType.None;

            if (elementAttr != null)
            {
                result.SerializationHint = elementAttr.XmlSerialization;
                result.Order = elementAttr.Order;
            }
          
            result.IsCollection = ReflectionHelper.IsTypedCollection(prop.PropertyType) && !prop.PropertyType.IsArray;

            // Get to the actual (native) type representing this element
            if (result.IsCollection) result.ElementType = ReflectionHelper.GetCollectionItemType(prop.PropertyType);
            if (ReflectionHelper.IsNullableType(result.ElementType)) result.ElementType = ReflectionHelper.GetNullableArgument(result.ElementType);
            result.IsPrimitive = isAllowedNativeTypeForDataTypeValue(result.ElementType);

            result.IsBackboneElement = result.ElementType.CanBeTreatedAsType(typeof(IBackboneElement));
            foundTypes.Add(result.ElementType);

            // Derive the C# type that represents which types are allowed for this element.
            // This may differ from the ImplementingType in several ways:
            // * for a choice, ImplementingType = Any, but FhirType[] contains the possible choices
            // * some elements (e.g. Extension.url) have ImplementingType = string, but FhirType = FhirUri, etc.
            var allowedTypes = getAttribute<Validation.AllowedTypesAttribute>(prop, version);
            if (allowedTypes != null)
            {
                result.IsOpen = allowedTypes.IsOpen;
                result.FhirType = allowedTypes.IsOpen ? (new[] { typeof(Element) }) : allowedTypes.Types;
            }
            else if (elementAttr?.TypeRedirect != null)
                result.FhirType = new[] { elementAttr.TypeRedirect };
            else
                result.FhirType = new[] { result.ElementType };

            // Check wether this property represents a native .NET type
            // marked to receive the class' primitive value in the fhir serialization
            // (e.g. the value from the Xml 'value' attribute or the Json primitive member value)
            if (result.IsPrimitive) result.RepresentsValueElement = isPrimitiveValueElement(elementAttr,prop);

            referredTypes = foundTypes;

#if USE_CODE_GEN
            result._getter = prop.GetValueGetter();
            result._setter = prop.GetValueSetter();
#else
            result._getter = instance => prop.GetValue(instance, null);
            result._setter = (instance,value) => prop.SetValue(instance, value, null);
#endif       
            return result;
        }

        private static string determinePropertyName(FhirElementAttribute elementAttr, PropertyInfo prop)
        {
            if(elementAttr != null && elementAttr.Name != null)
                return elementAttr.Name;
            else
                return lowerCamel(prop.Name);            
        }

        private static string lowerCamel(string p)
        {
            if (p == null) return p;

            var c = p[0];

            return Char.ToLowerInvariant(c) + p.Remove(0, 1);
        }


        private static string buildQualifiedPropName(PropertyInfo prop)
        {
            return prop.DeclaringType.Name + "." + prop.Name;
        }


        private static bool isPrimitiveValueElement(FhirElementAttribute valueElementAttr, PropertyInfo prop)
        {
            var isValueElement = valueElementAttr != null && valueElementAttr.IsPrimitiveValue;

            if(isValueElement && !isAllowedNativeTypeForDataTypeValue(prop.PropertyType))
                throw Error.Argument(nameof(prop), "Property {0} is marked for use as a primitive element value, but its .NET type ({1}) is not supported by the serializer.".FormatWith(buildQualifiedPropName(prop), prop.PropertyType.Name));

            return isValueElement;
        }

        public bool MatchesSuffixedName(string suffixedName)
        {
            if (suffixedName == null) throw Error.ArgumentNull(nameof(suffixedName));

            return Choice == ChoiceType.DatatypeChoice && suffixedName.ToUpperInvariant().StartsWith(Name.ToUpperInvariant());
        }

        public string GetChoiceSuffixFromName(string suffixedName)
        {
            if (suffixedName == null) throw Error.ArgumentNull(nameof(suffixedName));

            if (MatchesSuffixedName(suffixedName))
                return suffixedName.Remove(0, Name.Length);
            else
                throw Error.Argument(nameof(suffixedName), "The given suffixed name {0} does not match this property's name {1}".FormatWith(suffixedName, Name));
        }
     
        private static bool isAllowedNativeTypeForDataTypeValue(Type type)
        {
            // Special case, allow Nullable<enum>
            if (ReflectionHelper.IsNullableType(type))
                type = ReflectionHelper.GetNullableArgument(type);

            return type.IsEnum() ||
                    PrimitiveTypeConverter.CanConvert(type);
        }

        private Func<object, object> _getter;
        private Action<object, object> _setter;

        public object GetValue(object instance)
        {
            return _getter(instance);
        }

        public void SetValue(object instance, object value)
        {
            _setter(instance, value);
        }
    }
}
