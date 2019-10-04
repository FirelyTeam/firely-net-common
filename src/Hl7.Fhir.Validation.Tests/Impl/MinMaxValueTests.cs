using Hl7.Fhir.Validation.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hl7.Fhir.Validation.Tests.Impl
{
    [TestClass]
    public class MinMaxValueTests : SimpleAssertionTests
    {
        public MinMaxValueTests() : base(new MinMaxValue("MinMaxValueTests", null, MinMax.MinValue))
        {

        }

        [TestMethod]
        public void MyTestMethod()
        {

        }

    }
}
