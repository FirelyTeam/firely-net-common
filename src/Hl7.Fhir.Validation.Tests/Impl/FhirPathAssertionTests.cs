using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Impl;
using Hl7.Fhir.Validation.Schema;
using Hl7.FhirPath;
using Hl7.FhirPath.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hl7.Fhir.Validation.Tests.Impl
{
    [TestClass]
    public class FhirPathAssertionTests
    {
        private readonly FhirPathCompiler fpCompiler;

        public FhirPathAssertionTests()
        {
            var symbolTable = new SymbolTable();
            symbolTable.AddStandardFP();
            fpCompiler = new FhirPathCompiler(symbolTable);
        }

        [TestMethod]
        public void ValidateWithoutSettings()
        {
            var validatable = new FhirPathAssertion("test-1", "hasValue()");

            var input = ElementNode.ForPrimitive("test");

            var result = validatable.Validate(input, null);
        }

        [TestMethod]
        public void ValidateSuccess()
        {
            var validatable = new FhirPathAssertion("test-1", "$this = 'test'");

            var input = ElementNode.ForPrimitive("test");

            var result = validatable.Validate(input, new ValidationContext() { fpCompiler = fpCompiler });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Result.IsSuccessful, "the FhirPath Expression must be valid for this input");
        }

        [TestMethod]
        public void ValidateIncorrectFhirPath()
        {
            var validatable = new FhirPathAssertion("test-1", "this is not a fhirpath expression");

            var input = ElementNode.ForPrimitive("test");

            var result = validatable.Validate(input, new ValidationContext() { fpCompiler = fpCompiler });

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Result.IsSuccessful, "the FhirPath Expression must not be valid for this input");
        }

        [TestMethod]
        public void ValidateChildrenExists()
        {
            var humanName = ElementNode.Root("HumanName");
            humanName.Add("family", "Brown", "string");
            humanName.Add("given", "Joe", "string");
            humanName.Add("given", "Patrick", "string");

            var validatable = new FhirPathAssertion("test-1", "children().count() = 3");

            var result = validatable.Validate(humanName, new ValidationContext() { fpCompiler = fpCompiler });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Result.IsSuccessful, "the FhirPath Expression must not be valid for this input");
        }
    }
}
