using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hl7.Fhir.Validation.Tests.Schema
{
    [TestClass]
    public class MaxLengthTests
    {

        [TestMethod]
        public void LengthTooLong()
        {
            var validatable = new MaxLength(10);


            var node = ElementNode.ForPrimitive("12345678901");

            var result = validatable.Validate(node, null);

            Assert.IsFalse(result.Result.IsSuccessful);
        }

        [TestMethod]
        public void LengthCorrect()
        {
            var validatable = new MaxLength(10);


            var node = ElementNode.ForPrimitive("1234567890");

            var result = validatable.Validate(node, null);

            Assert.IsTrue(result.Result.IsSuccessful);
        }

    }
}
