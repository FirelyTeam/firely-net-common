using Hl7.Fhir.ElementModel;
using Hl7.Fhir.ElementModel.Types;
using Hl7.Fhir.Validation.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using P = Hl7.Fhir.ElementModel.Types;

namespace Hl7.Fhir.Validation.Impl.Tests
{
    [TestClass()]
    public class BindingAssertionTests
    {
        private readonly BindingAssertion _bindingAssertion;
        private readonly ValidationContext _validationContext;
        private readonly Mock<ITerminologyServiceNEW> _terminologyService;


        public BindingAssertionTests()
        {
            var valueSetUri = "http://hl7.org/fhir/ValueSet/data-absent-reason";
            _bindingAssertion = new BindingAssertion(valueSetUri, BindingAssertion.BindingStrength.Required);

            _terminologyService = new Mock<ITerminologyServiceNEW>();

            _validationContext = new ValidationContext() { TerminologyService = _terminologyService.Object };
        }

        private void SetupTerminologyServiceResult(Assertions result)
        {
            _terminologyService.Setup(ts => ts.ValidateCode(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Code>(), It.IsAny<Concept>(), It.IsAny<P.DateTime>(), true, It.IsAny<string>())).Returns(Task.FromResult(result));
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidValidationContextException), "InvalidValidationContextException was expected because the terminology service is absent")]
        public async Task NoTerminolyServicePresent()
        {
            var input = ElementNode.ForPrimitive(true);
            var vc = new ValidationContext();

            _ = await _bindingAssertion.Validate(input, vc).ConfigureAwait(false);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException), "No input is present")]
        public async Task NoInputPresent()
        {
            _ = await _bindingAssertion.Validate(null, _validationContext).ConfigureAwait(false);
        }

        [TestMethod()]
        public async Task ValidateTest()
        {
            var input = ElementNode.ForPrimitive(true);

            var result = await _bindingAssertion.Validate(input, _validationContext).ConfigureAwait(false);
        }

        private ElementNode createCoding(string system, string code, string display = null)
        {
            var codingValue = ElementNode.Root("Coding");
            codingValue.Add("system", system, "uri");
            codingValue.Add("code", code, "string");
            if (display is object)
                codingValue.Add("display", display, "string");

            return codingValue;
        }


        private ElementNode createConcept(ElementNode[] coding, string text = null)
        {
            var conceptValue = ElementNode.Root("CodeableConcept");

            foreach (var item in coding)
            {
                conceptValue.Add(item, "coding");
            }
            if (text is object)
                conceptValue.Add("text", text, "string");
            return conceptValue;
        }

        private ElementNode createQuantity(decimal value, string unit)
        {
            var quantityValue = ElementNode.Root("Quantity");
            quantityValue.Add("value", value);
            quantityValue.Add("code", unit);
            return quantityValue;
        }

        [TestMethod]
        public async Task ValidateWithCode()
        {
            SetupTerminologyServiceResult(Assertions.Success);
            var input = ElementNode.Root("code", value: "CD123");

            var result = await _bindingAssertion.Validate(input, _validationContext).ConfigureAwait(false);

            Assert.IsTrue(result.Result.IsSuccessful);
            _terminologyService.Verify(ts => ts.ValidateCode(
                It.IsAny<string>(), // canonical
                It.IsAny<string>(), // context
                "CD123", // code
                null, // system
                null, // version
                null, // display
                null, // coding
                null, // concept
                null, // date
                true,  // abstracy
                null // displayLanguage
             ), Times.Once());
        }

        [TestMethod]
        public async Task ValidateWithUri()
        {
            SetupTerminologyServiceResult(Assertions.Success);
            var input = ElementNode.Root("uri", value: "http://some.uri");

            var result = await _bindingAssertion.Validate(input, _validationContext).ConfigureAwait(false);

            Assert.IsTrue(result.Result.IsSuccessful);
            _terminologyService.Verify(ts => ts.ValidateCode(
                It.IsAny<string>(), // canonical
                It.IsAny<string>(), // context
                "http://some.uri", // code
                null, // system
                null, // version
                null, // display
                null, // coding
                null, // concept
                null, // date
                true,  // abstracy
                null // displayLanguage
             ), Times.Once());
        }

        [TestMethod]
        public async Task ValidateWithString()
        {
            SetupTerminologyServiceResult(Assertions.Success);
            var input = ElementNode.Root("string", value: "Some string");

            var result = await _bindingAssertion.Validate(input, _validationContext).ConfigureAwait(false);

            Assert.IsTrue(result.Result.IsSuccessful);
            _terminologyService.Verify(ts => ts.ValidateCode(
                It.IsAny<string>(), // canonical
                It.IsAny<string>(), // context
                "Some string", // code
                null, // system
                null, // version
                null, // display
                null, // coding
                null, // concept
                null, // date
                true,  // abstracy
                null // displayLanguage
             ), Times.Once());
        }

        [TestMethod]
        public async Task ValidateWithCoding()
        {
            SetupTerminologyServiceResult(Assertions.Success);

            var input = createCoding("http://terminology.hl7.org/CodeSystem/data-absent-reason", "masked");
            var result = await _bindingAssertion.Validate(input, _validationContext).ConfigureAwait(false);

            Assert.IsTrue(result.Result.IsSuccessful);
            _terminologyService.Verify(ts => ts.ValidateCode(
               It.IsAny<string>(), // canonical
               It.IsAny<string>(), // context
               null, // code
               null, // system
               null, // version
               null, // display
               It.Is<Code>(cd => cd.Value == "masked" && cd.System == "http://terminology.hl7.org/CodeSystem/data-absent-reason"), // coding
               null, // concept
               null, // date
               true,  // abstracy
               null // displayLanguage
            ), Times.Once());
        }

        [TestMethod]
        public async Task ValidateWithCodeableConcept()
        {
            SetupTerminologyServiceResult(Assertions.Success);
            var codings = new[] { createCoding("http://terminology.hl7.org/CodeSystem/data-absent-reason", "masked") ,
            createCoding("http://terminology.hl7.org/CodeSystem/data-absent-reason", "masked")};

            var input = createConcept(codings);

            var result = await _bindingAssertion.Validate(input, _validationContext).ConfigureAwait(false);

            Assert.IsTrue(result.Result.IsSuccessful);
            _terminologyService.Verify(ts => ts.ValidateCode(
                It.IsAny<string>(), // canonical
                It.IsAny<string>(), // context
                null, // code
                null, // system
                null, // version
                null, // display
                null, // coding
                It.IsNotNull<Concept>(), // concept
                null, // date
                true,  // abstracy
                null // displayLanguage
             ), Times.Once());
        }

        [TestMethod]
        public async Task ValidateWithQuantity()
        {
            SetupTerminologyServiceResult(Assertions.Success);

            var input = createQuantity(25, "s");
            var result = await _bindingAssertion.Validate(input, _validationContext).ConfigureAwait(false);

            Assert.IsTrue(result.Result.IsSuccessful);
            _terminologyService.Verify(ts => ts.ValidateCode(
               It.IsAny<string>(), // canonical
               It.IsAny<string>(), // context
               null, // code
               null, // system
               null, // version
               null, // display
               It.Is<Code>(cd => cd.Value == "s"), // coding
               null, // concept
               null, // date
               true,  // abstracy
               null // displayLanguage
            ), Times.Once());
        }

        [TestMethod]
        public async Task ValidateEmptyString()
        {
            var input = ElementNode.Root("string", value: "");

            var result = await _bindingAssertion.Validate(input, _validationContext).ConfigureAwait(false);

            Assert.IsFalse(result.Result.IsSuccessful);
            _terminologyService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ValidateCodingWithoutCode()
        {
            var input = createCoding("system", null, null);

            var result = await _bindingAssertion.Validate(input, _validationContext).ConfigureAwait(false);

            Assert.IsFalse(result.Result.IsSuccessful);
            _terminologyService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ValidateInvalidCoding()
        {
            SetupTerminologyServiceResult(Assertions.Failure);

            var input = createCoding("http://terminology.hl7.org/CodeSystem/data-absent-reason", "UNKNOWN");
            var result = await _bindingAssertion.Validate(input, _validationContext).ConfigureAwait(false);

            Assert.IsFalse(result.Result.IsSuccessful);
            _terminologyService.Verify(ts => ts.ValidateCode(
               It.IsAny<string>(), // canonical
               It.IsAny<string>(), // context
               null, // code
               null, // system
               null, // version
               null, // display
               It.Is<Code>(cd => cd.Value == "UNKNOWN" && cd.System == "http://terminology.hl7.org/CodeSystem/data-absent-reason"), // coding
               null, // concept
               null, // date
               true,  // abstracy
               null // displayLanguage
            ), Times.Once());
        }
    }
}