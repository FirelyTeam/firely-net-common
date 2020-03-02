using Hl7.Fhir.ElementModel;
using Hl7.FhirPath.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace HL7.FhirPath.Tests.Functions
{
    [TestClass]
    public class CollectionOperatorsTests
    {
        [TestMethod]
        public void Intersect()
        {
            var a = ElementNode.ForPrimitive("A");
            var b1 = ElementNode.ForPrimitive("B");
            var c = ElementNode.ForPrimitive("C");
            var b2 = ElementNode.ForPrimitive("B");

            var col1 = new ITypedElement[] { a, b1 };
            var col2 = new ITypedElement[] { c, b2 };
            var col3 = new ITypedElement[] { c };

            var result = col1.Intersect(col2);
            Assert.IsNotNull(result);
            Assert.AreEqual("B", result.First().Value);

            result = col2.Intersect(col1);
            Assert.IsNotNull(result);
            Assert.AreEqual("B", result.First().Value);

            result = col1.Intersect(col3);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }
    }
}
