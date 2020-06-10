using Hl7.Fhir.ElementModel;
using Hl7.Fhir.ElementModel.Functions;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Validation.Schema;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Impl
{
    public class Pattern : SimpleAssertion
    {
        private readonly ITypedElement _pattern;

        public Pattern(ITypedElement patternValue)
        {
            this._pattern = patternValue;
        }

        public Pattern(object fixedValue) : this(ElementNode.ForPrimitive(fixedValue)) { }

        public override string Key => "pattern[x]";

        public override object Value => _pattern;

        public override Task<Assertions> Validate(ITypedElement input, ValidationContext vc)
        {
            var result = Assertions.Empty + new Trace($"Validate with pattern {_pattern.ToJson()}");
            if (!input.Matches(_pattern))
            {
                return Task.FromResult(result + ResultAssertion.CreateFailure(new IssueAssertion(Issue.CONTENT_DOES_NOT_MATCH_PATTERN_VALUE, input.Location, $"Value does not match pattern '{_pattern.ToJson()}")));
            }

            return Task.FromResult(result + Assertions.Success);
        }


        public override JToken ToJson()
        {
            return new JProperty(Key, _pattern.ToJObject());
        }
    }
}
