using System.Linq;

namespace Hl7.Fhir.Validation.Schema
{
    public static class AssertionExtensions
    {
        public static Assertions AddResultAssertion(this Assertions assertions)
        {
            return assertions.OfType<IssueAssertion>().Any() ? assertions + ResultAssertion.Failure : assertions + ResultAssertion.Success;
        }
    }
}
