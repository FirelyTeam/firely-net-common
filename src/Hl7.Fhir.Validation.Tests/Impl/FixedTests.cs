using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model.Primitives;
using Hl7.Fhir.Validation.Impl;
using Hl7.Fhir.Validation.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Hl7.Fhir.Validation.Tests.Impl
{
    [TestClass]
    public class FixedTests
    {
        private IEnumerable<(object fixedValue, object input, bool expectedResult, string failureMessage)> TestData()
        {
            // integer
            yield return (fixedValue: 90, input: 91, expectedResult: false, failureMessage: "result must be false [int]");
            yield return (fixedValue: 90, input: 90, expectedResult: true, failureMessage: "result must be true [int]");

            // string
            yield return (fixedValue: "test", input: "testfailure", expectedResult: false, failureMessage: "result must be false [string]");
            yield return (fixedValue: "test", input: "test", expectedResult: true, failureMessage: "result must be true [string]");

            // date
            yield return (fixedValue: PartialDate.Parse("2019-09-05"), input: PartialDate.Parse("2019-09-04"), expectedResult: false, failureMessage: "result must be false [date]");
            yield return (fixedValue: PartialDate.Parse("2019-09-05"), input: PartialDate.Parse("2019-09-05"), expectedResult: true, failureMessage: "result must be true [date]");

            // boolean
            yield return (fixedValue: true, input: false, expectedResult: false, failureMessage: "result must be false [boolean]");
            yield return (fixedValue: true, input: true, expectedResult: true, failureMessage: "result must be true [boolean]");

            // mixed primitive types
            yield return (fixedValue: PartialDate.Parse("2019-09-05"), input: 20190905, expectedResult: false, failureMessage: "result must be false [mixed]");
        }

        [TestMethod]
        public void FixedTestcases()
        {
            foreach (var (fixedValue, input, expectedResult, failureMessage) in TestData())
            {
                var validatable = new Fixed("FixedTests.FixedTestcases", fixedValue);
                var result = validatable.Validate(ElementNode.ForPrimitive(input), new ValidationContext());

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Result.IsSuccessful == expectedResult, failureMessage);
            }
        }

        [TestMethod]
        public void FixedHumanName()
        {
            var fixedValue = ElementNode.Root("HumanName");
            fixedValue.Add("family", "Brown", "string");
            fixedValue.Add("given", "Joe", "string");
            fixedValue.Add("given", "Patrick", "string");

            var validatable = new Fixed("FixedTests.FixedHumanName", fixedValue);
            var result = validatable.Validate(ElementNode.ForPrimitive("Brown, Joe Patrick"), new ValidationContext());

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Result.IsSuccessful, "String and HumanName are different");
        }

        [TestMethod]
        public void FixedHumanNameDifferentInstance()
        {
            var fixedValue = ElementNode.Root("HumanName");
            fixedValue.Add("family", "Brown", "string");
            fixedValue.Add("given", "Joe", "string");
            fixedValue.Add("given", "Patrick", "string");

            var input = ElementNode.Root("HumanName");
            input.Add("family", "Brown", "string");
            input.Add("given", "Patrick", "string");
            input.Add("given", "Joe", "string");

            var validatable = new Fixed("FixedTests.FixedHumanNameDifferentInstance", fixedValue);
            var result = validatable.Validate(input, new ValidationContext());

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Result.IsSuccessful, "The input (HumanName) is slightly different than the fixed value");
        }
    }
}
