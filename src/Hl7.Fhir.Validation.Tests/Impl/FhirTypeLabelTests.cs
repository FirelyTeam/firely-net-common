using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Schema;
using Hl7.Fhir.Validation.Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Impl.Tests
{
    internal class FhirTypeLabelAssertionData : SimpleAssertionDataAttribute
    {
        public override IEnumerable<object[]> GetData()
        {
            yield return new object[]
            {
                new FhirTypeLabel("System.String"),
                ElementNode.ForPrimitive("Value of type System.String"),
                true, null, "Same type"
            };
            yield return new object[]
            {
                new FhirTypeLabel("string"),
                ElementNode.ForPrimitive(9),
                false, Issue.CONTENT_ELEMENT_HAS_INCORRECT_TYPE, "Not the same type"
            };
        }
    }

    [TestClass]
    public class FhirTypeLabelTests : SimpleAssertionTests
    {

        [DataTestMethod]
        [FhirTypeLabelAssertionData]
        public override Task SimpleAssertionTestcases(SimpleAssertion assertion, ITypedElement input, bool expectedResult, Issue expectedIssue, string failureMessage)
           => base.SimpleAssertionTestcases(assertion, input, expectedResult, expectedIssue, failureMessage);
    }
}