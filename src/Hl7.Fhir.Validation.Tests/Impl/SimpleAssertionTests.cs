using Hl7.Fhir.Validation.Schema;

namespace Hl7.Fhir.Validation.Tests.Impl
{
    public abstract class SimpleAssertionTests
    {
        protected readonly SimpleAssertion _validatable;

        public SimpleAssertionTests(SimpleAssertion simpleAssertion)
        {
            _validatable = simpleAssertion;
        }
    }
}
