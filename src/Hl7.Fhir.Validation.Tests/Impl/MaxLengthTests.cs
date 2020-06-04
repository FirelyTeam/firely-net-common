using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Impl;
using Hl7.Fhir.Validation.Schema;
using Hl7.Fhir.Validation.Tests.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Tests.Schema
{
    [TestClass]
    public class MaxLengthTests : SimpleAssertionTests
    {
        public MaxLengthTests() : base(new MaxLength("MaxLengthTests", 10)) { }

        [TestMethod]
        public async Task LengthTooLong()
        {
            var node = ElementNode.ForPrimitive("12345678901");

            var result = await _validatable.Validate(node, null).ConfigureAwait(false);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Result.IsSuccessful);
            var evidence = result.Result.Evidence.OfType<IssueAssertion>();
            Assert.AreEqual(1, evidence.Count());
            Assert.AreEqual(1005, evidence.Single().IssueNumber);
        }

        [TestMethod]
        public async Task LengthCorrect()
        {
            var node = ElementNode.ForPrimitive("1234567890");

            var result = await _validatable.Validate(node, null).ConfigureAwait(false);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Result.IsSuccessful);
        }

        [TestMethod]
        public async Task ValidateWithOtherThanString()
        {
            // TODO debatable: is MaxLength for an integer valid? It is now Undecided.
            var node = ElementNode.ForPrimitive(90);

            var result = await _validatable.Validate(node, null).ConfigureAwait(false);

            Assert.IsFalse(result.Result.IsSuccessful, "MaxLength constraint on a non-string primitive is undecided == not succesful");
        }

        [TestMethod]
        public async Task ValidateWithEmptyString()
        {
            var node = ElementNode.ForPrimitive("");

            var result = await _validatable.Validate(node, null).ConfigureAwait(false);

            Assert.IsTrue(result.Result.IsSuccessful, "MaxLength constraint on an empty string must be succesful");
        }

        [TestMethod]
        [ExpectedException(typeof(IncorrectElementDefinitionException), "A negative number was allowed.")]
        public void InitializeWithNegativeMaxLength()
        {
            new MaxLength("MaxLengthTests.InitializeWithNegativeMaxLength", -1);
        }
    }
}
