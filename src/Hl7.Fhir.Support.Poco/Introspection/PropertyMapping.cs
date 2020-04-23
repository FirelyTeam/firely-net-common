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
using Hl7.Fhir.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Hl7.Fhir.Introspection
{
    [System.Diagnostics.DebuggerDisplay(@"\{Name={Name} ElementType={ElementType.Name}}")]
    public class PropertyMapping : IElementDefinitionSummary
    {
        private PropertyMapping()
        {
            // no public constructors
        }

        public string Name { get; internal set; }

        public bool IsCollection { get; internal set; }

        /// <summary>
        /// The element is of an atomic .NET type, not a FHIR generated POCO.
        /// </summary>
        public bool IsPrimitive { get; private set; }

        /// <summary>
        /// The element is a primitive (<seealso cref="IsPrimitive"/>) and 
        /// represents the primitive `value` attribute/property in the FHIR serialization.
        /// </summary>
        public bool RepresentsValueElement { get; private set; }

        public bool InSummary { get; private set; }
        public bool IsMandatoryElement { get; private set; }

        /// <summary>
        /// The native type of the element.
        /// </summary>
        /// <remarks>If the element is a collection or is nullable, this reflects the
        /// collection item or the type that is made nullable respectively.
        /// </remarks>
        public Type ElementType { get; private set; }

        public int Order { get; private set; }

        public XmlRepresentation SerializationHint { get; private set; }

        /// <summary>
        /// True if this element is a choice type element.
        /// </summary>
        /// <remarks>These elements have names ending in [x] in the StructureDefinition
        /// and allow a (possibly restricted) set of types to be used. These are reflected
        /// in the <see cref="FhirType"/> property.</remarks>
        public bool IsDatatypeChoice { get; private set; }

        /// <summary>
        /// This element is a polymorphic Resource, any resource is allowed here.
        /// </summary>
        /// <remarks>These are elements like DomainResource.contained, Parameters.resource etc.</remarks>
        public bool IsResourceChoice { get; private set; }

        /// <summary>
        /// The list of possible FHIR types for this element, represented as native types.
        /// </summary>
        /// <remark> <para>
        /// These are the defined (choice) types for this element as specified in the
        /// FHIR data definitions. It is derived from the actual type in the POCO class and 
        /// the [AllowedTypes] attribute and may by a [DeclaredTypes] attribute.
        /// </para>
        /// <para>
        /// May be a non-FHIR .NET primitive type for value elements of
        /// primitive FHIR datatypes (e.g. FhirBoolean.Value) or other primitive
        /// attributes (e.g. Extension.url)
        /// </para>
        /// </remark>
        public Type[] FhirType { get; private set; }        // may be multiple if this is a choice

        /// <summary>
        /// True when the element is of type '*', e.g. Extension.value[x]. Any type is allowed.
        /// </summary>
        public bool IsOpen { get; private set; }

        private PropertyInfo _propInfo;
        private int _createdVersion;

        [Obsolete("Use TryCreate() instead.")]
        public static PropertyMapping Create(PropertyInfo prop, int version = int.MaxValue)
            => TryCreate(prop, out var mapping, version) ? mapping : null;

        public static bool TryCreate(PropertyInfo prop, out PropertyMapping result, int version)
        {
            if (prop == null) throw Error.ArgumentNull(nameof(prop));

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
            result.SerializationHint = elementAttr.XmlSerialization;
            result.Order = elementAttr.Order;
            result._propInfo = prop;
            result._createdVersion = version;

            var cardinalityAttr = getAttribute<Validation.CardinalityAttribute>(prop, version);
            result.IsMandatoryElement = cardinalityAttr != null ? cardinalityAttr.Min > 0 : false;

            result.IsCollection = ReflectionHelper.IsTypedCollection(prop.PropertyType) &&
                prop.PropertyType != typeof(string);           // prevent silly string:char[] confusion

            // Get to the actual (native) type representing this element
            result.ElementType = prop.PropertyType;
            if (result.IsCollection) result.ElementType = ReflectionHelper.GetCollectionItemType(prop.PropertyType);
            if (ReflectionHelper.IsNullableType(result.ElementType)) result.ElementType = ReflectionHelper.GetNullableArgument(result.ElementType);
            result.IsPrimitive = isAllowedNativeTypeForDataTypeValue(result.ElementType);

            // Determine which FHIR type represents this ElementType
            // This is normally just the ElementType itself, but can be overridden
            // with the [DeclaredType] attribute.
            var declaredType = getAttribute<DeclaredTypeAttribute>(prop, version);
            var fhirType = declaredType?.Type ?? result.ElementType;

            result.IsResourceChoice = fhirType.GetTypeInfo().IsAbstract &&
                                        fhirType.CanBeTreatedAsType(typeof(Resource));
            result.IsDatatypeChoice = fhirType.GetTypeInfo().IsAbstract &&
                                        fhirType.CanBeTreatedAsType(typeof(DataType));

            // The [AllowedElements] attribute can specify a set of allowed types
            // for this element. Take this list as the declared list of FHIR types.
            // If not present assume this is the declared FHIR type above
            var allowedTypes = getAttribute<AllowedTypesAttribute>(prop, version);

            result.IsOpen = allowedTypes?.IsOpen == true;
            result.FhirType = allowedTypes?.Types?.Any() == true ?
                allowedTypes.Types : new[] { fhirType };

            if (result.FhirType == null || !result.FhirType.Any())
                throw new InvalidOperationException();

            // Check wether this property represents a native .NET type
            // marked to receive the class' primitive value in the fhir serialization
            // (e.g. the value from the Xml 'value' attribute or the Json primitive member value)
            if (result.IsPrimitive) result.RepresentsValueElement = isPrimitiveValueElement(elementAttr, prop);

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

            if (isValueElement && !isAllowedNativeTypeForDataTypeValue(prop.PropertyType))
                throw Error.Argument(nameof(prop), "Property {0} is marked for use as a primitive element value, but its .NET type ({1}) " +
                    "is not supported by the serializer.".FormatWith(buildQualifiedPropName(prop), prop.PropertyType.Name));

            return isValueElement;

            string buildQualifiedPropName(PropertyInfo p) => p.DeclaringType.Name + "." + p.Name;
        }

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

        internal Action<object, object> Setter
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

        string IElementDefinitionSummary.ElementName => this.Name;

        bool IElementDefinitionSummary.IsCollection => this.IsCollection;

        bool IElementDefinitionSummary.IsRequired => this.IsMandatoryElement;

        bool IElementDefinitionSummary.InSummary => this.InSummary;

        bool IElementDefinitionSummary.IsChoiceElement => this.IsDatatypeChoice;

        bool IElementDefinitionSummary.IsResource => this.IsResourceChoice;

        string IElementDefinitionSummary.DefaultTypeName => null;
            
        ITypeSerializationInfo[] IElementDefinitionSummary.Type => throw new NotImplementedException();

        string IElementDefinitionSummary.NonDefaultNamespace => null;

        XmlRepresentation IElementDefinitionSummary.Representation =>
            SerializationHint != XmlRepresentation.None ?
            SerializationHint : XmlRepresentation.XmlElement;

        int IElementDefinitionSummary.Order => Order;

        private Action<object, object> _setter;


        public object GetValue(object instance) => Getter(instance);

        public void SetValue(object instance, object value) => Setter(instance, value);

        private ITypeSerializationInfo[] buildTypes()
        {
            var success = ClassMapping.TryCreate(FhirType[0], out var elementTypeMapping, _createdVersion);

            if (elementTypeMapping.IsNestedType)
            {
                var info = elementTypeMapping;
                return new ITypeSerializationInfo[] { info };
            }
            else
            {
                var names = FhirType.Select(ft => getFhirTypeName(ft));
                return names.Select(n => (ITypeSerializationInfo)new PocoTypeReferenceInfo(n)).ToArray();
            }

            string getFhirTypeName(Type ft)
            {
                if (ClassMapping.TryCreate(ft, out var tm, _createdVersion))
                    return tm.IsCodeOfT ? "code" : tm.Name;
                else
                    throw new NotSupportedException($"Type '{ft.Name}' is an allowed type for property " +
                        $"'{_propInfo.Name}' in '{_propInfo.DeclaringType.Name}', but it does not seem to" +
                        $"be a valid FHIR type POCO.");
            }
        }

        struct PocoTypeReferenceInfo : IStructureDefinitionReference
        {
            public PocoTypeReferenceInfo(string canonical)
            {
                ReferredType = canonical;
            }

            public string ReferredType { get; private set; }
        }
    }
}
