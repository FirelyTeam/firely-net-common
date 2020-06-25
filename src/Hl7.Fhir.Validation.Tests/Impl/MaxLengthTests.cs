using FluentAssertions;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Impl;
using Hl7.Fhir.Validation.Impl.Tests;
using Hl7.Fhir.Validation.Schema;
using Hl7.Fhir.Validation.Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Tests.Schema
{
    internal class MaxLengthAssertionData : SimpleAssertionDataAttribute
    {
        public override IEnumerable<object[]> GetData()
        {
            yield return new object[]
            {
                new MaxLength(10),
                ElementNode.ForPrimitive("12345678901"),
                false, Issue.CONTENT_ELEMENT_VALUE_TOO_LONG, "LengthTooLong"
            };
            yield return new object[]
            {
                new MaxLength(10),
                ElementNode.ForPrimitive("1234567890"),
                true, null, "Length correct"
            };
            yield return new object[]
            {
                new MaxLength(10),
                ElementNode.ForPrimitive("1"),
                true, null, "Length correct"
            };
            yield return new object[]
            {
                new MaxLength(10),
                ElementNode.ForPrimitive(""),
                true, null, "Empty string is correct"
            };
            // TODO debatable: is MaxLength for an integer valid? It is now Undecided.
            yield return new object[]
            {
                new MaxLength(10),
                ElementNode.ForPrimitive(90),
                false, null, "MaxLength constraint on a non-string primitive is undecided == not succesful"
            };
        }
    }

    [TestClass]
    public class MaxLengthTests : SimpleAssertionTests
    {
        [TestMethod]
        public void InvalidConstructors()
        {
            Action action = () => new MaxLength(0);
            action.Should().Throw<IncorrectElementDefinitionException>();

            action = () => new MaxLength(-9);
            action.Should().Throw<IncorrectElementDefinitionException>();
        }

        [TestMethod]
        public void CorrectConstructor()
        {
            var assertion = new MaxLength(4);

            assertion.Should().NotBeNull();
            assertion.Key.Should().Be("maxLength");
            assertion.Value.Should().Be(4);
        }

        [DataTestMethod]
        [MaxLengthAssertionData]
        public override Task SimpleAssertionTestcases(SimpleAssertion assertion, ITypedElement input, bool expectedResult, Issue expectedIssue, string failureMessage)
            => base.SimpleAssertionTestcases(assertion, input, expectedResult, expectedIssue, failureMessage);
    }
}
