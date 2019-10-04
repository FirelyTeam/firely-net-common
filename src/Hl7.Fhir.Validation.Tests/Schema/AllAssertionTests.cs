using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Impl;
using Hl7.Fhir.Validation.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Hl7.Fhir.Validation.Tests.Schema
{
    [TestClass]
    public class AllAssertionTests
    {
        private class SuccessAssertion : IAssertion, IValidatable
        {
            public JToken ToJson()
            {
                throw new System.NotImplementedException();
            }

            public Assertions Validate(ITypedElement input, ValidationContext vc)
            {
                return Assertions.Success + new TraceText("Success Assertion");
            }
        }

        private class FailureAssertion : IAssertion, IValidatable
        {
            public JToken ToJson()
            {
                throw new System.NotImplementedException();
            }

            public Assertions Validate(ITypedElement input, ValidationContext vc)
            {
                return Assertions.Failure + new TraceText("Failure Assertion");
            }
        }


        [TestMethod]
        public void SingleOperand()
        {
            var allAssertion = new AllAssertion(new SuccessAssertion());
            var result = allAssertion.Validate(null, null);
            Assert.IsTrue(result.Result.IsSuccessful);

            allAssertion = new AllAssertion(new FailureAssertion());
            result = allAssertion.Validate(null, null);
            Assert.IsFalse(result.Result.IsSuccessful);

        }

        [TestMethod]
        public void Combinations()
        {
            var allAssertion = new AllAssertion(new SuccessAssertion(), new FailureAssertion());
            var result = allAssertion.Validate(null, null);
            Assert.IsFalse(result.Result.IsSuccessful);

        }
    }
}
