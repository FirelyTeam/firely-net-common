using FluentAssertions;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.ElementModel.Types;
using Hl7.Fhir.Validation.Impl;
using Hl7.Fhir.Validation.Schema;
using Hl7.Fhir.Validation.Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Tests.Impl
{
    internal class FixedAssertionData : SimpleAssertionDataAttribute
    {
        public override IEnumerable<object[]> GetData()
        {
            // integer
            yield return new object[]
            {
                new Fixed(10),
                ElementNode.ForPrimitive(91),
                false, Issue.CONTENT_DOES_NOT_MATCH_FIXED_VALUE, "result must be false [int]"
            };
            yield return new object[]
            {
                new Fixed(90),
                ElementNode.ForPrimitive(90),
                true, null, "result must be true [int]"
            };
            // string
            yield return new object[]
            {
                new Fixed("test"),
                ElementNode.ForPrimitive("testfailure"),
                false, Issue.CONTENT_DOES_NOT_MATCH_FIXED_VALUE, "result must be false [string]"
            };
            yield return new object[]
            {
                new Fixed("test"),
                ElementNode.ForPrimitive("test"),
                true, null,"result must be true [string]"
            };
            // boolean
            yield return new object[]
            {
                new Fixed(true),
                ElementNode.ForPrimitive(false),
                false, Issue.CONTENT_DOES_NOT_MATCH_FIXED_VALUE, "result must be false [boolean]"
            };
            yield return new object[]
            {
                new Fixed(true),
                ElementNode.ForPrimitive(true),
                true, null, "result must be true [boolean]"
            };
            // mixed primitive types
            yield return new object[]
            {
                new Fixed(Date.Parse("2019-09-05")),
                ElementNode.ForPrimitive(20190905),
                false, Issue.CONTENT_DOES_NOT_MATCH_FIXED_VALUE, "result must be false [mixed]"
            };
            // Complex Types
            yield return new object[]
            {
                new Fixed(Foo.CreateHumanName("Brown", new[] { "Joe" } )),
                Foo.CreateHumanName("Brown", new[] { "Joe" } ),
                true, null, "The input should match: family name should be Brown, and given name is Joe"
            };
            yield return new object[]
            {
                new Fixed(Foo.CreateHumanName("Brown", new[] { "Joe" } )),
                ElementNode.ForPrimitive("Brown, Joe Patrick"),
                false, Issue.CONTENT_DOES_NOT_MATCH_FIXED_VALUE, "String and HumanName are different"
            };
            yield return new object[]
            {
                new Fixed(Foo.CreateHumanName("Brown", new[] { "Joe", "Patrick" } )),
                Foo.CreateHumanName("Brown", new[] { "Patrick", "Joe" } ),
                false, Issue.CONTENT_DOES_NOT_MATCH_FIXED_VALUE, "The input should not match the fixed"
            };
        }
    }

    [TestClass]
    public class FixedTests : SimpleAssertionTests
    {
        [TestMethod]
        public void InvalidConstructors()
        {
            Action action = () => new Fixed(null);
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void CorrectConstructor()
        {
            var assertion = new Fixed(4);

            assertion.Should().NotBeNull();
            assertion.Key.Should().Be("fixed[x]");
            assertion.Value.Should().BeAssignableTo<ITypedElement>();
        }

        [DataTestMethod]
        [FixedAssertionData]
        public override Task SimpleAssertionTestcases(SimpleAssertion assertion, ITypedElement input, bool expectedResult, Issue expectedIssue, string failureMessage)
            => base.SimpleAssertionTestcases(assertion, input, expectedResult, expectedIssue, failureMessage);
    }
}
