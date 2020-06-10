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
    public class ElementSchema : IElementSchema, IMergeable
    {
        public Uri Id { get; private set; }

        public Assertions Members { get; private set; }


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

        public async Task<Assertions> Validate(IEnumerable<ITypedElement> input, ValidationContext vc)
        {
            var members = Members.Where(vc?.Filter ?? (a => true));

            var multiAssertions = members.OfType<IGroupValidatable>();
            var singleAssertions = members.OfType<IValidatable>();

            var multiResults = await multiAssertions
                                        .Select(ma => ma.Validate(input, vc)).AggregateAsync();

            var singleResult = await input.Select(elt => singleAssertions.ValidateAsync(elt, vc)).AggregateAsync();
            return multiResults + singleResult;


            //TODO: can we do this as well? Makes it a bit shorter..
            //return await members.Select(m => m.Validate(input, vc)).AggregateAsync();
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

        public IMergeable Merge(IMergeable other) =>
            other is ElementSchema schema ? new ElementSchema(this.Members + schema.Members)
                : throw Error.InvalidOperation($"Internal logic failed: tried to merge an ElementSchema with a {other.GetType().Name}");
    }
}
