/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Validation.Schema;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Impl
{
    public class RegExAssertion : SimpleAssertion
    {
        private readonly string _pattern;
        private readonly Regex _regex;

        public RegExAssertion(string pattern)
        {
            _pattern = pattern;
            _regex = new Regex(pattern);
        }

        public override string Key => "regex";

        public override object Value => _pattern;

        public override Task<Assertions> Validate(ITypedElement input, ValidationContext vc)
        {
            if (_pattern != null)
            {
                var regex = new Regex(_pattern);
                var value = toStringRepresentation(input);
                var success = Regex.Match(value, $"^{_regex}$").Success;

                if (!success)
                {
                    return Task.FromResult(Assertions.Empty + ResultAssertion.CreateFailure(new IssueAssertion(Issue.CONTENT_ELEMENT_INVALID_PRIMITIVE_VALUE, input.Location, $"Value '{value}' does not match regex '{regex}'")));
                }
            }

            return Task.FromResult(Assertions.Success);
        }

        private string toStringRepresentation(ITypedElement vp)
        {
            return vp == null || vp.Value == null ?
                null :
                PrimitiveTypeConverter.ConvertTo<string>(vp.Value);
        }

    }
}
