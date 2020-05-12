using Hl7.Fhir.Model;
using Hl7.Fhir.Validation.Impl;
using Hl7.Fhir.Validation.Schema;
using Hl7.Fhir.Validation.Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Tests.Impl
{
    [TestClass]
    public class MinValueTests : SimpleAssertionTests
    {
        public MinValueTests() : base(new MinMaxValue("MinValueTests", PrimitiveTypeExtensions.ToTypedElement<Integer, int?>(4), MinMax.MinValue))
        {

        }

        [TestMethod]
        public async Task CompareWithOtherPrimitive()
        {
            var result = await _validatable.Validate(PrimitiveTypeExtensions.ToTypedElement<FhirString, string>("a string"), new ValidationContext());

            Assert.IsFalse(result.Result.IsSuccessful);

        }

        [TestMethod]
        public async Task LessThen()
        {
            var result = await _validatable.Validate(PrimitiveTypeExtensions.ToTypedElement<Integer, int?>(3), new ValidationContext());

            Assert.IsFalse(result.Result.IsSuccessful);
        }

        [TestMethod]
        public async Task Equals()
        {
            var result = await _validatable.Validate(PrimitiveTypeExtensions.ToTypedElement<Integer, int?>(4), new ValidationContext());

            Assert.IsTrue(result.Result.IsSuccessful);
        }

        [TestMethod]
        public async Task GreaterThen()
        {
            var result = await _validatable.Validate(PrimitiveTypeExtensions.ToTypedElement<Integer, int?>(5), new ValidationContext());

            Assert.IsTrue(result.Result.IsSuccessful);
        }

    }

    [TestClass]
    public class MaxValueTests : SimpleAssertionTests
    {
        public MaxValueTests() : base(new MinMaxValue("MaxValueTests", PrimitiveTypeExtensions.ToTypedElement<Date, string>("1905-08-23"), MinMax.MaxValue))
        {

        }

        [TestMethod]
        public async Task CompareWithOtherPrimitive()
        {
            var result = await _validatable.Validate(PrimitiveTypeExtensions.ToTypedElement<Integer, int?>(2), new ValidationContext());

            Assert.IsFalse(result.Result.IsSuccessful);

        }

        [TestMethod]
        public async Task LessThen()
        {
            var result = await _validatable.Validate(PrimitiveTypeExtensions.ToTypedElement<Date, string>("1905-01-01"), new ValidationContext());

            Assert.IsTrue(result.Result.IsSuccessful);
        }

        [TestMethod]
        public async Task PartialEquals()
        {
            var result = await _validatable.Validate(PrimitiveTypeExtensions.ToTypedElement<Date, string>("1905"), new ValidationContext());

            Assert.IsTrue(result.Result.IsSuccessful);
        }

        [TestMethod]
        public async Task Equals()
        {
            var result = await _validatable.Validate(PrimitiveTypeExtensions.ToTypedElement<Date, string>("1905-08-23"), new ValidationContext());

            Assert.IsTrue(result.Result.IsSuccessful);
        }

        [TestMethod]
        public async Task GreaterThen()
        {
            var result = await _validatable.Validate(PrimitiveTypeExtensions.ToTypedElement<Date, string>("1905-12-31"), new ValidationContext());

            Assert.IsFalse(result.Result.IsSuccessful);
        }

        [TestMethod]
        public async Task PartialGreaterThen()
        {
            var result = await _validatable.Validate(PrimitiveTypeExtensions.ToTypedElement<Date, string>("1906"), new ValidationContext());

            Assert.IsFalse(result.Result.IsSuccessful);
        }

    }
}
