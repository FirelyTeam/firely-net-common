using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Schema;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Impl
{
    public class FhirTypeLabel : SimpleAssertion
    {
        public readonly string Label;

        public FhirTypeLabel(string label, string location) : base(location)
        {
            Label = label;
        }

        public override string Key => "fhir-type-label";

        public override object Value => Label;

        public override Task<Assertions> Validate(ITypedElement input, ValidationContext vc)
        {
            // TODO use ModelInfo

            var result = Assertions.Empty;

            result += input?.InstanceType == Label ?
                new ResultAssertion(ValidationResult.Success) :
                ResultAssertion.CreateFailure(new IssueAssertion(-1, $"Type of instance ({input?.InstanceType}) is not valid at location {input?.Location}.", IssueSeverity.Error));

            return Task.FromResult(result);
        }
    }
}
