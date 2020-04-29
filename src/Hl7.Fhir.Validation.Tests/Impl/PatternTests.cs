using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model.Primitives;
using Hl7.Fhir.Validation.Impl;
using Hl7.Fhir.Validation.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Tests.Impl
{
    [TestClass]
    public class PatternTests
    {
        private IEnumerable<(object patternValue, object input, bool expectedResult, string failureMessage)> TestData()
        {
            // integer
            yield return (patternValue: 90, input: 91, expectedResult: false, failureMessage: "result must be false [int]");
            yield return (patternValue: 90, input: 90, expectedResult: true, failureMessage: "result must be true [int]");

            // string
            yield return (patternValue: "test", input: "testfailure", expectedResult: false, failureMessage: "result must be false [string]");
            yield return (patternValue: "test", input: "test", expectedResult: true, failureMessage: "result must be true [string]");

            // date
            yield return (patternValue: PartialDate.Parse("2019-09-05"), input: PartialDate.Parse("2019-09-04"), expectedResult: false, failureMessage: "result must be false [date]");
            yield return (patternValue: PartialDate.Parse("2019-09-05"), input: PartialDate.Parse("2019-09-05"), expectedResult: true, failureMessage: "result must be true [date]");

            // boolean
            yield return (patternValue: true, input: false, expectedResult: false, failureMessage: "result must be false [boolean]");
            yield return (patternValue: true, input: true, expectedResult: true, failureMessage: "result must be true [boolean]");

            // mixed primitive types
            yield return (patternValue: PartialDate.Parse("2019-09-05"), input: 20190905, expectedResult: false, failureMessage: "result must be false [mixed]");
        }

        [TestMethod]
        public async Task PrimitivePatternTestcases()
        {
            foreach (var (patternValue, input, expectedResult, failureMessage) in TestData())
            {
                var validatable = new Fixed("PatternTests.PatternTestcases", patternValue);
                var result = await validatable.Validate(ElementNode.ForPrimitive(input), new ValidationContext());

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Result.IsSuccessful == expectedResult, failureMessage);
            }
        }

        [TestMethod]
        public async Task PatternHumanName()
        {
            var patternValue = ElementNode.Root("HumanName");
            patternValue.Add("family", "Brown", "string");
            patternValue.Add("given", "Joe", "string");
            patternValue.Add("given", "Patrick", "string");

            var validatable = new Fixed("PatternTests.PatternHumanName", patternValue);
            var result = await validatable.Validate(ElementNode.ForPrimitive("Brown, Joe Patrick"), new ValidationContext());

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Result.IsSuccessful, "String and HumanName are different");
        }

        [TestMethod]
        public async Task ComplexTypePattern()
        {
            var patternValue = ElementNode.Root("HumanName");
            patternValue.Add("family", "Brown", "string");
            patternValue.Add("given", "Joe", "string");

            var input = ElementNode.Root("HumanName");
            input.Add("family", "Brown", "string");
            input.Add("given", "Joe", "string");
            input.Add("given", "Patrick", "string");

            var validatable = new Pattern("PatternTests.ComplexTypePattern", patternValue);
            var result = await validatable.Validate(input, new ValidationContext());

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Result.IsSuccessful, "The input should match the pattern: family name should be Brown, and given name is Joe");
        }

        [TestMethod]
        public async Task NotMatchingComplexTypePattern()
        {
            var patternValue = ElementNode.Root("HumanName");
            patternValue.Add("family", "Brown", "string");
            patternValue.Add("given", "Joe", "string");
            patternValue.Add("given", "Donald", "string");

            var input = ElementNode.Root("HumanName");
            input.Add("family", "Brown", "string");
            input.Add("given", "Joe", "string");

            var validatable = new Pattern("PatternTests.NotMatchingComplexTypePattern", patternValue);
            var result = await validatable.Validate(input, new ValidationContext());

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Result.IsSuccessful, "The input should not match the pattern");
        }

    }
}
