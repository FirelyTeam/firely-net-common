using FluentAssertions;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Tests.Support
{
    public abstract class SimpleAssertionTests
    {
        public virtual async Task SimpleAssertionTestcases(SimpleAssertion assertion, ITypedElement input, bool expectedResult, Issue expectedIssue, string failureMessage)
        {
            var result = await assertion.Validate(input, ValidationContext.CreateDefault()).ConfigureAwait(false);

            result.Should().NotBeNull();
            result.Result.IsSuccessful.Should().Be(expectedResult, failureMessage);

            if (expectedResult == false && expectedIssue is object)
            {
                result.Result.Evidence.OfType<IssueAssertion>().Should().Contain(ia => ia.IssueNumber == expectedIssue.IssueNumber);
            }
        }
    }
}
