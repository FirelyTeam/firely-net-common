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
        public static IEnumerable<object[]> TestData()
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
        public async Task PrimitivePatternTestcases(object patternValue, object input, bool expectedResult, string failureMessage)
        {
            var validatable = new Pattern(patternValue);
            var result = await validatable.Validate(ElementNode.ForPrimitive(input), ValidationContext.CreateDefault()).ConfigureAwait(false);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Result.IsSuccessful == expectedResult, failureMessage);
        }

        [TestMethod]
        public async Task PatternHumanName()
        {
            var patternValue = ElementNode.Root("HumanName");
            patternValue.Add("family", "Brown", "string");
            patternValue.Add("given", "Joe", "string");
            patternValue.Add("given", "Patrick", "string");

            var validatable = new Pattern(patternValue);
            var result = await validatable.Validate(ElementNode.ForPrimitive("Brown, Joe Patrick"), new ValidationContext()).ConfigureAwait(false);

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

            var validatable = new Pattern(patternValue);
            var result = await validatable.Validate(input, new ValidationContext()).ConfigureAwait(false);

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

            var validatable = new Pattern(patternValue);
            var result = await validatable.Validate(input, new ValidationContext()).ConfigureAwait(false);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Result.IsSuccessful, "The input should not match the pattern");
        }

    }
}
