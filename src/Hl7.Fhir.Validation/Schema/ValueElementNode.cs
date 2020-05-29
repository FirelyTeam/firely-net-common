using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Language;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.Validation.Schema
{
    public class ValueElementNode : BaseTypedElement
    {
        private bool _extraChild = false;

        public ValueElementNode(ITypedElement wrapped, bool valuedChild = false) : base(wrapped)
        {
            _extraChild = valuedChild;
        }

        public override string Name => _extraChild ? "value" : base.Name;

        public override string InstanceType => _extraChild ? TypeSpecifier.ForNativeType(Wrapped.Value.GetType()).FullName : base.InstanceType;

        public override string Location => _extraChild ? $"{Wrapped.Location}.value" : base.Location;

        public override IEnumerable<ITypedElement> Children(string name = null)
        {
            if ((Value is object) && Char.IsLower(InstanceType[0]) && !base.Children(name).Any())
            {
                yield return new ValueElementNode(this, true);
            }
            else
            {
                foreach (var child in base.Children(name))
                    yield return child;
            }
        }

    }

    public static class ValueElementNodeExtensions
    {
        public static ITypedElement AddValueNode(this ITypedElement node)
        {
            return node is ValueElementNode ? node : new ValueElementNode(node);
        }
    }
}
