using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model;
using Hl7.Fhir.Specification;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Hl7.Fhir.ElementModel
{
    public static class DynamicExtensions
    {
        public static DynamicObject Dynamic(this ElementNode n, IStructureDefinitionSummaryProvider prov) 
            => new DynamicElementNode(n, prov);
    }

    internal class DynamicElementNode : DynamicObject
    {
        private readonly ElementNode _wrapped;
        private readonly IStructureDefinitionSummaryProvider _prov;

        public DynamicElementNode(ElementNode wrapped, IStructureDefinitionSummaryProvider prov)
        {
            _wrapped = wrapped ?? throw new ArgumentNullException(nameof(wrapped));
            _prov = prov;
        }


        public override IEnumerable<string> GetDynamicMemberNames()
           => base.GetDynamicMemberNames().Union(_wrapped.Children().Select(c => c.Name));

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            ElementNode element;

            if (value is ElementNode en)
                element = en;
            else if (value is ITypedElement ite)
                element = ElementNode.FromElement(ite, true);
            else if (value is Base b)
                element = ElementNode.FromElement(b.ToTypedElement(_prov), true);
            else
                return false;

            var existingMember = (ElementNode)_wrapped.Children(binder.Name).FirstOrDefault();

            if (existingMember != null)
            {
                _wrapped.Replace(_prov, existingMember, element);
            }
            else
                _wrapped.Add(_prov, element, binder.Name);

            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (base.TryGetMember(binder, out result)) return true;

            result = ((ElementNode)_wrapped.Children(binder.Name).FirstOrDefault()).Dynamic(_prov);

            return result != null;
        }

        // ook setindex overriden
        //public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        //{
        //    if (indexes.Length == 1 && indexes[0] is string key)
        //    {
        //        result = (ElementNode)_wrapped.Children(key).FirstOrDefault();
        //        return result != null;
        //    }
        //    else
        //        return base.TryGetIndex(binder, indexes, out result);
        //}

        //public override bool TryConvert(ConvertBinder binder, out object result)
        //{
        //    var poco = this._wrapped.ToPoco()
        //    return base.TryConvert(binder, out result);
        //}
    }

}
