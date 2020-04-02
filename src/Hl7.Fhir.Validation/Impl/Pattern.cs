using Hl7.Fhir.ElementModel;
using Hl7.Fhir.ElementModel.Functions;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Validation.Schema;
using Newtonsoft.Json.Linq;

namespace Hl7.Fhir.Validation.Impl
{
    public class Pattern : SimpleAssertion
    {
        private readonly ITypedElement _pattern;

        public Pattern(string location, ITypedElement patternValue) : base(location)
        {
            this._pattern = patternValue;
        }

        public Pattern(string location, object fixedValue) : this(location, ElementNode.ForPrimitive(fixedValue)) { }

        protected override string Key => "pattern[x]";

        protected override object Value => _pattern;

        public override Assertions Validate(ITypedElement input, ValidationContext vc)
        {
            var result = Assertions.Empty + new Trace($"Validate with pattern {_pattern.ToJson()}");
            if (!input.Matches(_pattern))
            {
                return result + Assertions.Failure + new IssueAssertion(1009, Location, $"Value does not match pattern '{_pattern.ToJson()}", IssueSeverity.Error);

            }

            return result + Assertions.Success;
        }


        public override JToken ToJson()
        {
            return new JProperty(Key, _pattern.ToJObject());
        }
    }
}
