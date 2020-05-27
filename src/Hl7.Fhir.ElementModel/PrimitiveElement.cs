/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Language;
using Hl7.Fhir.Model.Primitives;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.ElementModel
{
    internal class PrimitiveElement : ITypedElement, IElementDefinitionSummary, IStructureDefinitionSummary
    {
        // [20190827 EK] Hack, allow a Quantity as a "primitive" value in ITypedElement.Value for now, so
        // we can at least continue to integrate the changes from the dead branch into 2.0
        // We need to have Quantity implement ITypedElement itself.
        internal static PrimitiveElement ForQuantity(Quantity value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            return new PrimitiveElement
            {
                Value = value,
                InstanceType = TypeSpecifier.Quantity.FullName,
                Name = "@QuantityAsPrimitiveValue@"
            };
        }

        private PrimitiveElement()
        {            
        }

        public PrimitiveElement(object value, string name = null, bool useFullTypeName = false)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var systemType = NamedTypeSpecifier.ForNativeType(value.GetType());
            if(!TypeSpecifier.PrimitiveTypes.Contains(systemType))
                throw new ArgumentException("The supplied value cannot be represented with a System primitive.", nameof(value));
           
            Value = Any.ConvertToSystemValue(value);
            InstanceType = useFullTypeName ? systemType.FullName : systemType.Name;
            Name = name ?? "@primitivevalue@";
        }

        public string Name { get; private set; }

        public object Value { get; private set; }

        public string InstanceType { get; private set; }

        public string Location => Name;

        public IElementDefinitionSummary Definition => this;

        string IElementDefinitionSummary.ElementName => Name;

        bool IElementDefinitionSummary.IsCollection => false;

        bool IElementDefinitionSummary.IsRequired => false;

        bool IElementDefinitionSummary.InSummary => false;

        bool IElementDefinitionSummary.IsChoiceElement => false;

        bool IElementDefinitionSummary.IsResource => false;

        string IElementDefinitionSummary.DefaultTypeName => null;

        ITypeSerializationInfo[] IElementDefinitionSummary.Type => new[] { this };

        string IElementDefinitionSummary.NonDefaultNamespace => null;

        XmlRepresentation IElementDefinitionSummary.Representation => XmlRepresentation.XmlAttr;

        int IElementDefinitionSummary.Order => 0;

        string IStructureDefinitionSummary.TypeName => InstanceType;

        bool IStructureDefinitionSummary.IsAbstract => false;

        bool IStructureDefinitionSummary.IsResource => false;

        public override bool Equals(object obj) => obj is ITypedElement ite && Equals(ite.Value, Value);

        public override int GetHashCode() => Value?.GetHashCode() ?? 0;

        public ITypedElement Clone() => new PrimitiveElement(Value);

        public IEnumerable<ITypedElement> Children(string name = null) => Enumerable.Empty<ITypedElement>();
        IReadOnlyCollection<IElementDefinitionSummary> IStructureDefinitionSummary.GetElements() =>
#if NET40
            new ReadOnlyList<IElementDefinitionSummary>();
#else
            new List<IElementDefinitionSummary>();
#endif

        public override string ToString() => Value != null ? PrimitiveTypeConverter.ConvertTo<string>(Value) : "";

    }
}
