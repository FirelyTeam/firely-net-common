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
    public class FixedTests
    {
        private static IEnumerable<object[]> TestData()
        {
            // integer
            yield return new object[] { 90, 91, false, "result must be false [int]" };
            yield return new object[] { 90, 90, true, "result must be true [int]" };

            // string
            yield return new object[] { "test", "testfailure", false, "result must be false [string]" };
            yield return new object[] { "test", "test", true, "result must be true [string]" };

            // date
            yield return new object[] { PartialDate.Parse("2019-09-05"), PartialDate.Parse("2019-09-04"), false, "result must be false [date]" };
            yield return new object[] { PartialDate.Parse("2019-09-05"), PartialDate.Parse("2019-09-05"), true, "result must be true [date]" };

            // boolean
            yield return new object[] { true, false, false, "result must be false [boolean]" };
            yield return new object[] { true, true, true, "result must be true [boolean]" };

            // mixed primitive types
            yield return new object[] { PartialDate.Parse("2019-09-05"), 20190905, false, "result must be false [mixed]" };
        }

        [DataTestMethod]
        [DynamicData(nameof(TestData), DynamicDataSourceType.Method)]
        public async Task FixedTestcases(object fixedValue, object input, bool expectedResult, string failureMessage)
        {
            var validatable = new Fixed("FixedTests.FixedTestcases", fixedValue);
            var result = await validatable.Validate(ElementNode.ForPrimitive(input), new ValidationContext());

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Result.IsSuccessful == expectedResult, failureMessage);
        }

        [TestMethod]
        public async Task FixedHumanName()
        {
            var fixedValue = ElementNode.Root("HumanName");
            fixedValue.Add("family", "Brown", "string");
            fixedValue.Add("given", "Joe", "string");
            fixedValue.Add("given", "Patrick", "string");

            var validatable = new Fixed("FixedTests.FixedHumanName", fixedValue);
            var result = await validatable.Validate(ElementNode.ForPrimitive("Brown, Joe Patrick"), new ValidationContext());

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Result.IsSuccessful, "String and HumanName are different");
        }

        [TestMethod]
        public async Task FixedHumanNameDifferentInstance()
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
            var result = await validatable.Validate(input, new ValidationContext());

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Result.IsSuccessful, "The input (HumanName) is slightly different than the fixed value");
        }
    }
}
