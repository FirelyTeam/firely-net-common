/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.ElementModel.Functions;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Validation.Schema;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Impl
{
    public class Fixed : SimpleAssertion
    {
        private readonly ITypedElement _fixed;

        public Fixed(string location, ITypedElement fixedValue) : base(location)
        {
            this._fixed = fixedValue;
        }

        public Fixed(string location, object fixedValue) : this(location, ElementNode.ForPrimitive(fixedValue)) { }

        public override string Key => "fixed[x]";

        public override object Value => _fixed;

        public override Task<Assertions> Validate(ITypedElement input, ValidationContext vc)
        {
            if (!EqualityOperators.IsEqualTo(_fixed, input))
            {
                return Task.FromResult(Assertions.Failure + new IssueAssertion(121, input.Location, $"Value is not exactly equal to fixed value '{_fixed.Value}'", IssueSeverity.Error));
            }

            return Task.FromResult(Assertions.Success);
        }

        public override JToken ToJson()
        {
            return new JProperty(Key, _fixed.ToJObject());
        }
    }
}