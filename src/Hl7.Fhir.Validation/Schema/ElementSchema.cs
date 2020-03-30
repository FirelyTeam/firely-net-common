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
    public class ElementSchema : IAssertion, IMergeable, IGroupValidatable
    {
        public Uri Id { get; private set; }

        public readonly Assertions Members;


        public ElementSchema(Assertions assertions)
        {
            Members = assertions;
        }

        public ElementSchema(params IAssertion[] assertions) : this(new Assertions(assertions)) { }

        public ElementSchema(IEnumerable<IAssertion> assertions) : this(new Assertions(assertions)) { }

        public ElementSchema(Uri id, params IAssertion[] assertions) : this(assertions) => Id = id;

        public ElementSchema(Uri id, IEnumerable<IAssertion> assertions) : this(assertions) => Id = id;

        public ElementSchema(Uri id, Assertions assertions) : this(assertions) => Id = id;

        private static Uri buildUri(string uri) => new Uri(uri, UriKind.RelativeOrAbsolute);

        public ElementSchema(string id, params IAssertion[] assertions) : this(assertions)
        {
            Id = buildUri(id);
        }

        public ElementSchema(string id, IEnumerable<IAssertion> assertions) : this(assertions)
        {
            Id = buildUri(id);
        }

        public ElementSchema(string id, Assertions assertions) : this(assertions)
        {
            Id = buildUri(id);
        }

        public bool IsEmpty => !Members.Any();

        public Assertions Validate(IEnumerable<ITypedElement> input, ValidationContext vc)
        {
            var multiAssertions = Members.OfType<IGroupValidatable>();
            var singleAssertions = Members.OfType<IValidatable>();

            var multiResults = multiAssertions
                                .Select(ma => ma.Validate(input, vc));

            var singleResults = input
                            .Select(elt => collectPerInstance(elt));

            return collect(multiResults.Union(singleResults));

            Assertions collectPerInstance(ITypedElement elt) =>
                collect(from assert in singleAssertions
                        select assert.Validate(elt, vc));

            Assertions collect(IEnumerable<Assertions> bunch) => bunch.Aggregate(Assertions.Empty, (sum, other) => sum += other);
        }

        public JToken ToJson()
        {
            var result = new JObject();
            if (Id != null) result.Add(new JProperty("$id", Id.ToString()));
            result.Add(Members.Select(mem => nest(mem.ToJson())));
            return result;

            JToken nest(JToken mem) =>
                mem is JObject ? new JProperty("nested", mem) : mem;
        }

        public ElementSchema With(params IAssertion[] additional) => With(additional.AsEnumerable());

        public ElementSchema With(IEnumerable<IAssertion> additional) =>
            new ElementSchema(this.Id, this.Members.Union(additional));

        public IMergeable Merge(IMergeable other) =>
            other is ElementSchema schema ? new ElementSchema(this.Members + schema.Members)
                : throw Error.InvalidOperation($"Internal logic failed: tried to merge an ElementSchema with a {other.GetType().Name}");
    }
}
