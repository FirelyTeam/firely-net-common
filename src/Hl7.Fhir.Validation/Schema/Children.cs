/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Utility;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Schema
{
    public class Children : IAssertion, IMergeable, IValidatable//, ICollectable
    {
        private readonly Lazy<IReadOnlyDictionary<string, IAssertion>> _childList;

        public Children(params (string name, IAssertion assertion)[] children)
        {
            _childList = new Lazy<IReadOnlyDictionary<string, IAssertion>>(() => toROD(children));
        }

        private IReadOnlyDictionary<string, IAssertion> toROD(IEnumerable<(string, IAssertion)> children)
        {
            var lookup = new Dictionary<string, IAssertion>();
            foreach (var (name, assertion) in children)
                lookup.Add(name, assertion);
#if NET40
            return lookup.ToReadOnlyDictionary();
#else
            return lookup;
#endif
        }


        public Children(IReadOnlyDictionary<string, IAssertion> children)
        {
            _childList = new Lazy<IReadOnlyDictionary<string, IAssertion>>(() => children);
        }

        public Children(Func<IReadOnlyDictionary<string, IAssertion>> childGenerator)
        {
            _childList = new Lazy<IReadOnlyDictionary<string, IAssertion>>(childGenerator);
        }

        public IReadOnlyDictionary<string, IAssertion> ChildList => _childList.Value;

        public IAssertion Lookup(string name) =>
            ChildList.TryGetValue(name, out var child) ? child : null;

        public IEnumerable<Assertions> Collect() => new Assertions(this).Collection;

        public IMergeable Merge(IMergeable other)
        {
            if (other is Children cd)
            {
                var mergedChildren = from name in names()
                                     let left = this.Lookup(name)
                                     let right = cd.Lookup(name)
                                     select (name, merge(left, right));
                return new Children(() => toROD(mergedChildren));
            }
            else
                throw Error.InvalidOperation($"Internal logic failed: tried to merge Children with an {other.GetType().Name}");

            IEnumerable<string> names() => ChildList.Keys.Union(cd.ChildList.Keys).Distinct();

            IAssertion merge(IAssertion l, IAssertion r)
            {
                if (l == null) return r;
                if (r == null) return l;

                return new ElementSchema(l, r);
            }
        }

        public JToken ToJson() =>
            new JProperty("children", new JObject() { ChildList.Select(child =>
                new JProperty(child.Key, child.Value.ToJson().MakeNestedProp())) });

        public async Task<Assertions> Validate(ITypedElement input, ValidationContext vc)
        {
            var element = input.AddValueNode();

            var result = Assertions.Empty;

            if (element.Value is null && !element.Children().Any())
            {
                result += new IssueAssertion(1000, element.Location, "Element must not be empty", IssueSeverity.Error);
            }

            // new style:
            //var filteredChildern = input.ChildrenIncValue().Where(child => NameMatches(assertion.Key, child));
            //result = await ChildList.Values.Select(assertion => assertion.Validate(input, vc)).AggregateAsync();

            foreach (var assertion in ChildList)
            {
                var childElements = element.Children().Where(child => NameMatches(assertion.Key, child)).ToList();

                result += await assertion.Value.Validate(childElements, vc);
            }

            // todo restanten, which are not part of the definition?

            return result;
        }

        private bool NameMatches(string name, ITypedElement instanceElement)
        {
            var definedName = name;

            // simple direct match
            if (definedName == instanceElement.Name) return true;

            // match where definition path includes a type suffix (typeslice shorthand)
            // example: path Patient.deceasedBoolean matches Patient.deceased (with type 'boolean')
            if (definedName == instanceElement.Name + instanceElement.InstanceType.Capitalize()) return true;

            // match where definition path is a choice (suffix '[x]'), in this case
            // match the path without the suffix against the name
            if (definedName.EndsWith("[x]"))
            {
                if (definedName.Substring(0, definedName.Length - 3) == instanceElement.Name) return true;
            }

            return false;
        }
    }
}
