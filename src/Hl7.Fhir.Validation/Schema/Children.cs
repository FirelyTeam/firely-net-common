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
    public class Children : IAssertion, IMergeable, IValidatable
    {
        private readonly Lazy<IReadOnlyDictionary<string, IAssertion>> _childList;
        private readonly bool _allowAdditionalChildren;

        public Children(bool allowAdditionalChildren, params (string name, IAssertion assertion)[] children) : this(() => toROD(children), allowAdditionalChildren)
        {
        }

        public Children(IReadOnlyDictionary<string, IAssertion> children, bool allowAdditionalChildren = false) : this(() => children, allowAdditionalChildren)
        {
        }

        public Children(Func<IReadOnlyDictionary<string, IAssertion>> childGenerator, bool allowAdditionalChildren = false)
        {
            _childList = new Lazy<IReadOnlyDictionary<string, IAssertion>>(childGenerator);
            _allowAdditionalChildren = allowAdditionalChildren;
        }

        private static IReadOnlyDictionary<string, IAssertion> toROD(IEnumerable<(string, IAssertion)> children)
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

        public IReadOnlyDictionary<string, IAssertion> ChildList => _childList.Value;

        public IAssertion Lookup(string name) =>
            ChildList.TryGetValue(name, out var child) ? child : null;

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
                result += ResultAssertion.CreateFailure(new IssueAssertion(Issue.CONTENT_ELEMENT_MUST_HAVE_VALUE_OR_CHILDREN, element.Location, "Element must not be empty"));
            }

            var matchResult = ChildNameMatcher.Match(ChildList, element);
            if (matchResult.UnmatchedInstanceElements.Any() && !_allowAdditionalChildren)
            {
                var elementList = String.Join(",", matchResult.UnmatchedInstanceElements.Select(e => "'" + e.Name + "'"));
                result += ResultAssertion.CreateFailure(new IssueAssertion(Issue.CONTENT_ELEMENT_HAS_UNKNOWN_CHILDREN, null, $"Encountered unknown child elements {elementList} for definition '{"TODO: definition.Path"}'"));
            }

            result += await matchResult.Matches.Select(m => m.Assertion.Validate(m.InstanceElements, vc)).AggregateAsync();
            return result;
        }
    }

    internal class ChildNameMatcher
    {
        public static MatchResult Match(IReadOnlyDictionary<string, IAssertion> assertions, ITypedElement instanceParent)
        {
            var elementsToMatch = instanceParent.Children().ToList();

            List<Match> matches = new List<Match>();

            foreach (var assertion in assertions)
            {
                var match = new Match() { Assertion = assertion.Value, InstanceElements = new List<ITypedElement>() };

                // Special case is the .value of a primitive fhir type, this is represented
                // as the "Value" of the IValueProvider interface, not as a real child
                //if (definitionElement.Current.IsPrimitiveValueConstraint())
                //{
                //    if (instanceParent.Value != null)
                //        match.InstanceElements.Add(instanceParent);
                //}
                //else
                //{
                var found = elementsToMatch.Where(ie => NameMatches(assertion.Key, ie)).ToList();

                match.InstanceElements.AddRange(found);
                elementsToMatch.RemoveAll(e => found.Contains(e));

                matches.Add(match);
            }

            MatchResult result = new MatchResult
            {
                Matches = matches,
                UnmatchedInstanceElements = elementsToMatch
            };

            return result;
        }

        private static bool NameMatches(string name, ITypedElement instanceElement)
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

    internal class MatchResult
    {
        public List<Match> Matches;
        public List<ITypedElement> UnmatchedInstanceElements;
    }

    internal class Match
    {
        public IAssertion Assertion;
        public List<ITypedElement> InstanceElements;
    }
}
