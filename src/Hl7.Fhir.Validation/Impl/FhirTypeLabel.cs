using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Schema;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Impl
{
    public class FhirTypeLabel : SimpleAssertion
    {
        public readonly string Label;

        public FhirTypeLabel(string label)
        {
            Label = label;
        }

        public override string Key => "fhir-type-label";

        public override object Value => Label;

        public override Task<Assertions> Validate(ITypedElement input, ValidationContext vc)
        {
            // TODO use ModelInfo 
            // ModelInfo.IsInstanceTypeFor(input?.InstanceType);

            var result = Assertions.Empty;

            result += input?.InstanceType == Label ?
                new ResultAssertion(ValidationResult.Success) :
                ResultAssertion.CreateFailure(new IssueAssertion(Issue.CONTENT_ELEMENT_HAS_INCORRECT_TYPE, input?.Location, $"Type of instance ({input?.InstanceType}) is not valid at location {input?.Location}."));

            return Task.FromResult(result);
        }
    }
}
