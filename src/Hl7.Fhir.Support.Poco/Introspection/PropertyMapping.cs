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
using System.Threading;

namespace Hl7.Fhir.Introspection
{
    [System.Diagnostics.DebuggerDisplay(@"\{Name={Name} ElementType={ElementType.Name}}")]
    public class PropertyMapping
    {
        public string Name { get; internal set; }

        public bool IsCollection { get; internal set; }

        /// <summary>
        /// The element is of an atomic .NET type, not a FHIR generated POCO.
        /// </summary>
        public bool IsPrimitive { get; private set; }

        /// <summary>
        /// The element is a primitive (<seealso cref="IsPrimitive"/>) and represents the `value` attribute.
        /// </summary>
        public bool RepresentsValueElement { get; private set; }

        public bool InSummary { get; private set; }
        public bool IsMandatoryElement { get; private set; }

        public Type ElementType { get; private set; }

        public int Order { get; private set; }

        public XmlRepresentation SerializationHint { get; private set; }

        /// <summary>
        /// True if this element is a choice or a Resource subtype (e.g in Resource.contained)
        /// </summary>
        public ChoiceType Choice { get; private set; }
        
        /// <summary>
        /// The list of possible FHIR types for this element, represented as C# types
        /// </summary>
        /// <remark>May differ from the actual type of the property if this is a choice
        /// (in which case the property is of type Element) or if a TypeRedirect attribute was applied. Useful
        /// for when the type in the FHIR specification differs from the actual type in the class.</remark>
        public Type[] FhirType { get; private set; }        // may be multiple if this is a choice

        /// <summary>
        /// True when the element is of type '*', e.g. Extension.value[x]. Any type is allowed.
        /// </summary>
        public bool IsOpen { get; private set; }

        private PropertyInfo _propInfo;


        public static bool TryCreate(PropertyInfo prop, out PropertyMapping mapping, int version = int.MaxValue) => TryCreate(prop, out mapping, out var _, version);

        [Obsolete("Use TryCreate() instead.")]
        public static PropertyMapping Create(PropertyInfo prop, int version = int.MaxValue) => TryCreate(prop, out var mapping, version) ? mapping : null;

        internal static bool TryCreate(PropertyInfo prop, out PropertyMapping result, out IList<Type> referredTypes, int version)
        {
            if (prop == null) throw Error.ArgumentNull(nameof(prop));
            
            referredTypes = new List<Type>();
            result = new PropertyMapping();

            // If there is no [FhirElement] on the property, skip it
            var elementAttr = getAttribute<FhirElementAttribute>(prop, version);
            if (elementAttr == null) return false;

            // If there is an explicit [NotMapped] on the property, skip it
            // (in combination with `Since` useful to remove a property from the serialization)
            var notmappedAttr = getAttribute<NotMappedAttribute>(prop, version);
            if (notmappedAttr != null) return false;

            result.Name = elementAttr.Name;
            result.InSummary = elementAttr.InSummary;
            result.Choice = elementAttr.Choice;
            result.SerializationHint = elementAttr.XmlSerialization;
            result.Order = elementAttr.Order;
            result._propInfo = prop;

            var cardinalityAttr = getAttribute<Validation.CardinalityAttribute>(prop, version);
            result.IsMandatoryElement = cardinalityAttr != null ? cardinalityAttr.Min > 0 : false;
         
            result.IsCollection = ReflectionHelper.IsTypedCollection(prop.PropertyType) && !prop.PropertyType.IsArray;

            // Get to the actual (native) type representing this element
            result.ElementType = prop.PropertyType;
            if (result.IsCollection) result.ElementType = ReflectionHelper.GetCollectionItemType(prop.PropertyType);
            if (ReflectionHelper.IsNullableType(result.ElementType)) result.ElementType = ReflectionHelper.GetNullableArgument(result.ElementType);
            result.IsPrimitive = isAllowedNativeTypeForDataTypeValue(result.ElementType);
            referredTypes.Add(result.ElementType);

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
            else if (elementAttr.TypeRedirect != null)
                result.FhirType = new[] { elementAttr.TypeRedirect };
            else
                result.FhirType = new[] { result.ElementType };

            // Check wether this property represents a native .NET type
            // marked to receive the class' primitive value in the fhir serialization
            // (e.g. the value from the Xml 'value' attribute or the Json primitive member value)
            if (result.IsPrimitive) result.RepresentsValueElement = isPrimitiveValueElement(elementAttr,prop);

            return true;
        }

        private static T getAttribute<T>(PropertyInfo p, int version) where T : Attribute
        {
            return ReflectionHelper.GetAttributes<T>(p).LastOrDefault(isRelevant);

            bool isRelevant(Attribute a) => a is IFhirVersionDependent vd ? vd.AppliesToVersion(version) : true;
        }


        private static bool isPrimitiveValueElement(FhirElementAttribute valueElementAttr, PropertyInfo prop)
        {
            var isValueElement = valueElementAttr != null && valueElementAttr.IsPrimitiveValue;

            if(isValueElement && !isAllowedNativeTypeForDataTypeValue(prop.PropertyType))
                throw Error.Argument(nameof(prop), "Property {0} is marked for use as a primitive element value, but its .NET type ({1}) " +
                    "is not supported by the serializer.".FormatWith(buildQualifiedPropName(prop), prop.PropertyType.Name));

            return isValueElement;

            string buildQualifiedPropName(PropertyInfo p) => p.DeclaringType.Name + "." + p.Name;
        }

        //public bool MatchesSuffixedName(string suffixedName)
        //{
        //    if (suffixedName == null) throw Error.ArgumentNull(nameof(suffixedName));

        //    return Choice == ChoiceType.DatatypeChoice && suffixedName.ToUpperInvariant().StartsWith(Name.ToUpperInvariant());
        //}

        //public string GetChoiceSuffixFromName(string suffixedName)
        //{
        //    if (suffixedName == null) throw Error.ArgumentNull(nameof(suffixedName));

        //    if (MatchesSuffixedName(suffixedName))
        //        return suffixedName.Remove(0, Name.Length);
        //    else
        //        throw Error.Argument(nameof(suffixedName), "The given suffixed name {0} does not match this property's name {1}".FormatWith(suffixedName, Name));
        //}
     
        private static bool isAllowedNativeTypeForDataTypeValue(Type type)
        {
            // Special case, allow Nullable<enum>
            if (ReflectionHelper.IsNullableType(type))
                type = ReflectionHelper.GetNullableArgument(type);

            return type.IsEnum() ||
                    PrimitiveTypeConverter.CanConvert(type);
        }

        internal Func<object, object> Getter
        {
            get
            {
#if USE_CODE_GEN
                LazyInitializer.EnsureInitialized(ref _getter, () => _propInfo.GetValueGetter());
#else
                LazyInitializer.EnsureInitialized(ref _getter, () => instance => _propInfo.GetValue(instance, null));
#endif
                return _getter;
            }
        }

        private Func<object, object> _getter;

        internal Action<object,object> Setter
        {
            get
            {
#if USE_CODE_GEN
                LazyInitializer.EnsureInitialized(ref _setter, () => _propInfo.GetValueSetter());
#else
                LazyInitializer.EnsureInitialized(ref _setter, () => (instance, value) => _propInfo.SetValue(instance, value, null));
#endif
                return _setter;
            }
        }

        private Action<object, object> _setter;


        public object GetValue(object instance) => Getter(instance);

        public void SetValue(object instance, object value) => Setter(instance, value);
    }
}
