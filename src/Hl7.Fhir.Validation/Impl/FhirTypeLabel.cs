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
            => Task.FromResult(input.InstanceType == Label ? Assertions.Success : Assertions.Failure); // TODO use ModelInfo

    }
}
