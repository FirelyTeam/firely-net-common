using Hl7.Fhir.ElementModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hl7.FhirPath.Functions;
using System.Linq;

namespace HL7.FhirPath.Tests
{
    [TestClass]
    public class CollectionTests
    {
        [TestMethod]
        public void TestIntersect()
        {
            var left = ElementNode.CreateList(1, 3, 3, 5, 6);
            var right = ElementNode.CreateList(3, 5, 5, 6, 8);
            CollectionAssert.AreEqual(ElementNode.CreateList(3, 5, 6).ToList(),
                    left.Intersect(right).ToList());
        }

        [TestMethod]
        public void TestExclude()
        {
            var left = ElementNode.CreateList(1, 3, 3, 5, 6);
            var right = ElementNode.CreateList(5, 6);
            CollectionAssert.AreEqual(ElementNode.CreateList(1, 3, 3).ToList(),
                    left.Exclude(right).ToList());
        }

    }
}
