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

        public Assertions Validate(ITypedElement input, ValidationContext vc)
        {
            var result = Assertions.Empty;

            foreach (var assertion in ChildList)
            {
                var childElements = input.ChildrenIncValue().Where(child => NameMatches(assertion.Key, child)).ToList();

                switch (assertion.Value)
                {
                    case IValidatable validatable:
                        result += validatable.Validate(childElements.SingleOrDefault(), vc);
                        break;
                    case IGroupValidatable groupvalidatable:
                        {
                            var a = groupvalidatable.Validate(childElements, vc);
                            foreach (var item in a)
                            {
                                result += item.Item1;
                            }
                            break;
                        }
                }
            }

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

    public static class TypeElementExtensions
    {
        public static IEnumerable<ITypedElement> ChildrenIncValue(this ITypedElement instance)
        {
            foreach (var child in instance.Children())
            {
                yield return child;

            }
            if (instance.InstanceType == "string" || instance.InstanceType == "instant")
            {
                yield return new ValueElementNode(instance.Value, instance.Location);
            }
        }
    }
}
