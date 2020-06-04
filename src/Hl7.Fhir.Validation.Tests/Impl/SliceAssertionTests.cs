using Hl7.Fhir.Validation.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hl7.Fhir.Validation.Impl.Tests
{
    [TestClass()]
    public class SliceAssertionTests
    {
        [TestMethod()]
        public void ConstructorTest()
        {
            var slice = new SliceAssertion.Slice("slice", ResultAssertion.Success, ResultAssertion.Success);
            var assertion = new SliceAssertion(false, slice);

            Assert.AreEqual(1, assertion.Slices.Length);
            Assert.AreEqual(false, assertion.Ordered);
            Assert.IsNotNull(assertion.Default);
        }
    }
}