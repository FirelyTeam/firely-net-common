using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Impl;
using Hl7.Fhir.Validation.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

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

            public Task<Assertions> Validate(ITypedElement input, ValidationContext vc)
            {
                return Task.FromResult(Assertions.Success + new TraceText("Success Assertion"));
            }
        }

        private class FailureAssertion : IAssertion, IValidatable
        {
            public JToken ToJson()
            {
                throw new System.NotImplementedException();
            }

            public Task<Assertions> Validate(ITypedElement input, ValidationContext vc)
            {
                return Task.FromResult(Assertions.Failure + new TraceText("Failure Assertion"));
            }
        }


        [TestMethod]
        public async Task SingleOperand()
        {
            var allAssertion = new AllAssertion(new SuccessAssertion());
            var result = await allAssertion.Validate(null, null).ConfigureAwait(false);
            Assert.IsTrue(result.Result.IsSuccessful);

            allAssertion = new AllAssertion(new FailureAssertion());
            result = await allAssertion.Validate(null, null).ConfigureAwait(false);
            Assert.IsFalse(result.Result.IsSuccessful);

        }

        [TestMethod]
        public async Task Combinations()
        {
            var allAssertion = new AllAssertion(new SuccessAssertion(), new FailureAssertion());
            var result = await allAssertion.Validate(null, null).ConfigureAwait(false);
            Assert.IsFalse(result.Result.IsSuccessful);

        }
    }
}
