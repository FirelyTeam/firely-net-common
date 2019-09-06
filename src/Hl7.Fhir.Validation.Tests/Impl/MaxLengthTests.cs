using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Impl;
using Hl7.Fhir.Validation.Schema;
using Hl7.Fhir.Validation.Tests.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hl7.Fhir.Validation.Tests.Schema
{
    [TestClass]
    public class MaxLengthTests : SimpleAssertionTests
    {
        public MaxLengthTests() : base(new MaxLength(10)) { }

        [TestMethod]
        public void LengthTooLong()
        {
            var node = ElementNode.ForPrimitive("12345678901");

            var result = _validatable.Validate(node, null);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.IsFalse(result.Result.IsSuccessful);
            Assert.AreEqual(1, result.Result.Evidence.Length);
            Assert.IsInstanceOfType(result.Result.Evidence[0], typeof(MaxLength));

        }

        [TestMethod]
        public void LengthCorrect()
        {
            var node = ElementNode.ForPrimitive("1234567890");

            var result = _validatable.Validate(node, null);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Result.IsSuccessful);
        }

        [TestMethod]
        public void ValidateWithOtherThanString()
        {
            var node = ElementNode.ForPrimitive(90);

            var result = _validatable.Validate(node, null);

            Assert.IsFalse(result.Result.IsSuccessful);
        }

        [TestMethod]
        [ExpectedException(typeof(IncorrectElementDefinitionException), "A negative number was allowed.")]
        public void InitializeWithNegativeMaxLength()
        {
            var validatable = new MaxLength(-1);

        }
    }
}
