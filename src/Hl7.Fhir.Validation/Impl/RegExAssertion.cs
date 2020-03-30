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

namespace Hl7.Fhir.Validation.Impl
{
    public class RegExAssertion : SimpleAssertion
    {
        private readonly string _pattern;
        private readonly Regex _regex;

        public RegExAssertion(string location, string pattern) : base(location)
        {
            _pattern = pattern;
            _regex = new Regex(pattern);
        }

        protected override string Key => "regex";

        protected override object Value => _pattern;

        public override Assertions Validate(ITypedElement input, ValidationContext vc)
        {
            if (_pattern != null)
            {
                var regex = new Regex(_pattern);
                var value = toStringRepresentation(input);
                var success = Regex.Match(value, $"^{_regex}$").Success;

                if (!success)
                {
                    return Assertions.Failure + new IssueAssertion(1006, input.Location, $"Value '{value}' does not match regex '{regex}'", IssueSeverity.Error);
                }
            }

            return Assertions.Success;
        }

        private string toStringRepresentation(ITypedElement vp)
        {
            return vp == null || vp.Value == null ?
                null :
                PrimitiveTypeConverter.ConvertTo<string>(vp.Value);
        }

    }
}
