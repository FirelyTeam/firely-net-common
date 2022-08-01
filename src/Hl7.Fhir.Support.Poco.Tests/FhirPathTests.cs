using Hl7.Fhir.ElementModel;
using Hl7.Fhir.FhirPath;
using Hl7.Fhir.Model;
using Hl7.FhirPath;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Hl7.Fhir.Support.Poco.Tests
{
    [TestClass]
    public class FhirPathTests
    {
        [TestMethod]
        public void TestFpFunctions()
        {
            // FHIR specific function exists on POCO
            var fhirData = new FhirString("hello!");
            Assert.IsTrue(fhirData.IsTrue("hasValue()"));

            // FHIR specific function does not work for ITypedElement extension methods
            var data = ElementNode.ForPrimitive("hello!");
            Assert.ThrowsException<ArgumentException>(() => data.IsTrue("hasValue()"));
        }
    }

}
