using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Language;
using Hl7.Fhir.Specification;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.Validation.Schema
{
    public class ValueElementNode : ITypedElement
    {
        public ValueElementNode(object value, string location)
        {
            Name = "value";
            var primitiveTypeName = TypeSpecifier.ForNativeType(value.GetType());
            InstanceType = primitiveTypeName.FullName;
            Location = $"{location}.value";
            Value = value;
        }

        public string Name { get; private set; }

        public object Value { get; private set; }

        public string InstanceType { get; private set; }

        public string Location { get; private set; }

        public IElementDefinitionSummary Definition => null;

        public IEnumerable<ITypedElement> Children(string name = null) => Enumerable.Empty<ITypedElement>();

    }
}
