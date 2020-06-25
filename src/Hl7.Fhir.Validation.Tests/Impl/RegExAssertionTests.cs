using FluentAssertions;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Schema;
using Hl7.Fhir.Validation.Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Impl.Tests
{
    internal class RegExAssertionData : SimpleAssertionDataAttribute
    {
        public override IEnumerable<object[]> GetData()
        {
            yield return new object[] { new RegExAssertion("[0-9]"), ElementNode.ForPrimitive(1), true, null, "result must be true '[0-9]'" };
            yield return new object[] { new RegExAssertion("[0-9]"), ElementNode.ForPrimitive("a"), false, Issue.CONTENT_ELEMENT_INVALID_PRIMITIVE_VALUE, "result must be false '[0-9]'" };

            yield return new object[]
            {
                new RegExAssertion(@"^((\+31)|(0031)|0)(\(0\)|)(\d{1,3})(\s|\-|)(\d{8}|\d{4}\s\d{4}|\d{2}\s\d{2}\s\d{2}\s\d{2})$"),
                ElementNode.ForPrimitive("+31(0)612345678"), true, null, "result must be true (Dutch phonenumber"
            };
        }
    }

    [TestClass]
    public class RegExAssertionTests : SimpleAssertionTests
    {
        [TestMethod]
        public void InvalidConstructors()
        {
            Action action = () => new RegExAssertion(null);
            action.Should().Throw<ArgumentNullException>();

            action = () => new RegExAssertion("__2398@)ahdajdlka ad ***********INVALID REGEX");
            action.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void CorrectConstructor()
        {
            var assertion = new RegExAssertion("[0-9]");

            assertion.Should().NotBeNull("valid regex");
            assertion.Key.Should().Be("regex");
            assertion.Value.Should().Be("[0-9]");
        }

        [DataTestMethod]
        [RegExAssertionData]
        public override async Task SimpleAssertionTestcases(SimpleAssertion assertion, ITypedElement input, bool expectedResult, Issue expectedIssue, string failureMessage)
            => await base.SimpleAssertionTestcases(assertion, input, expectedResult, expectedIssue, failureMessage);

    }
}