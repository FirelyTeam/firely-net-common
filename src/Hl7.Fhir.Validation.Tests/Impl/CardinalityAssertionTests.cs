using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Impl;
using Hl7.Fhir.Validation.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hl7.Fhir.Validation.Tests.Impl
{
    [TestClass]
    public class CardinalityAssertionTests
    {
        [ExpectedException(typeof(IncorrectElementDefinitionException), "min should be a positive number")]
        [TestMethod]
        public void IncorrectConstructorArguments1()
        {
            _ = new CardinalityAssertion(-1, null);
        }

        [TestMethod]
        public void IncorrectConstructorArguments2()
        {
            _ = new CardinalityAssertion(null, null);

            // no failures expected
        }

        [TestMethod]
        public void IncorrectConstructorArguments3()
        {
            _ = new CardinalityAssertion(0, "*");

            // no failures expected
        }

        [TestMethod]
        public void IncorrectConstructorArguments4()
        {
            _ = new CardinalityAssertion(0, null);

            // no failures expected
        }

        [TestMethod]
        public void IncorrectConstructorArguments5()
        {
            _ = new CardinalityAssertion(0, "1");

            // no failures expected
        }

        [TestMethod]
        public void IncorrectConstructorArguments6()
        {
            _ = new CardinalityAssertion(0, "*");

            // no failures expected
        }

        [ExpectedException(typeof(IncorrectElementDefinitionException), "max should be '*'")]
        [TestMethod]
        public void IncorrectConstructorArguments7()
        {
            _ = new CardinalityAssertion(0, "invalid");
        }

        [ExpectedException(typeof(IncorrectElementDefinitionException), "max should be positive number")]
        [TestMethod]
        public void IncorrectConstructorArguments8()
        {
            _ = new CardinalityAssertion(0, "-1");
        }

        [TestMethod]
        public void InRange()
        {
            var cardinality = new CardinalityAssertion(0, "3");

            var list = cardinality.Validate(ElementNode.CreateList("1", 1, 9L), null);




        }
    }
}
