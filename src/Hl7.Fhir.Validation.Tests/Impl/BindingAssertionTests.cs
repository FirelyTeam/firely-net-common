using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model;
using Hl7.Fhir.Model.Primitives;
using Hl7.Fhir.Validation.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

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
            _bindingAssertion = new BindingAssertion("BindingAssertionTests", valueSetUri, BindingAssertion.BindingStrength.Required);

            _terminologyService = new Mock<ITerminologyServiceNEW>();

            _validationContext = new ValidationContext() { TerminologyService = _terminologyService.Object };
        }

        private void SetupTerminologyServiceResult(Assertions result)
        {
            _terminologyService.Setup(ts => ts.ValidateCode(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ICoding>(), It.IsAny<IConcept>(), It.IsAny<PartialDateTime?>(), true, It.IsAny<string>())).Returns(Task.FromResult(result));
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidValidationContextException), "Expected InvalidValidationContextException was expected because the terminology service is absent")]
        public void NoTerminolyServicePresent()
        {
            var input = ElementNode.ForPrimitive(true);
            var vc = new ValidationContext();

            var result = _bindingAssertion.Validate(input, vc);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException), "No input is present")]
        public void NoInputPresent()
        {
            _ = _bindingAssertion.Validate(null, _validationContext);
        }

        [TestMethod()]
        public void ValidateTest()
        {
            var input = ElementNode.ForPrimitive(true);

            var result = _bindingAssertion.Validate(input, _validationContext);
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
        public async void ValidateWithCode()
        {
            SetupTerminologyServiceResult(new Assertions(ResultAssertion.Success));
            var input = ElementNode.Root("code", value: "CD123");

            var result = await _bindingAssertion.Validate(input, _validationContext);

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
        public async void ValidateWithUri()
        {
            SetupTerminologyServiceResult(new Assertions(ResultAssertion.Success));
            var input = ElementNode.Root("uri", value: "http://some.uri");

            var result = await _bindingAssertion.Validate(input, _validationContext);

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
        public async void ValidateWithString()
        {
            SetupTerminologyServiceResult(new Assertions(ResultAssertion.Success));
            var input = ElementNode.Root("string", value: "Some string");

            var result = await _bindingAssertion.Validate(input, _validationContext);

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
        public async void ValidateWithCoding()
        {
            SetupTerminologyServiceResult(new Assertions(ResultAssertion.Success));

            var input = createCoding("http://terminology.hl7.org/CodeSystem/data-absent-reason", "masked");
            var result = await _bindingAssertion.Validate(input, _validationContext);

            Assert.IsTrue(result.Result.IsSuccessful);
            _terminologyService.Verify(ts => ts.ValidateCode(
               It.IsAny<string>(), // canonical
               It.IsAny<string>(), // context
               null, // code
               null, // system
               null, // version
               null, // display
               It.Is<ICoding>(cd => cd.Code == "masked" && cd.System == "http://terminology.hl7.org/CodeSystem/data-absent-reason"), // coding
               null, // concept
               null, // date
               true,  // abstracy
               null // displayLanguage
            ), Times.Once());
        }

        [TestMethod]
        public async void ValidateWithCodeableConcept()
        {
            SetupTerminologyServiceResult(new Assertions(ResultAssertion.Success));
            var codings = new[] { createCoding("http://terminology.hl7.org/CodeSystem/data-absent-reason", "masked") ,
            createCoding("http://terminology.hl7.org/CodeSystem/data-absent-reason", "masked")};

            var input = createConcept(codings);

            var result = await _bindingAssertion.Validate(input, _validationContext);

            Assert.IsTrue(result.Result.IsSuccessful);
            _terminologyService.Verify(ts => ts.ValidateCode(
                It.IsAny<string>(), // canonical
                It.IsAny<string>(), // context
                null, // code
                null, // system
                null, // version
                null, // display
                null, // coding
                It.IsNotNull<IConcept>(), // concept
                null, // date
                true,  // abstracy
                null // displayLanguage
             ), Times.Once());
        }

        [TestMethod]
        public async void ValidateWithQuantity()
        {
            SetupTerminologyServiceResult(new Assertions(ResultAssertion.Success));

            var input = createQuantity(25, "s");
            var result = await _bindingAssertion.Validate(input, _validationContext);

            Assert.IsTrue(result.Result.IsSuccessful);
            _terminologyService.Verify(ts => ts.ValidateCode(
               It.IsAny<string>(), // canonical
               It.IsAny<string>(), // context
               null, // code
               null, // system
               null, // version
               null, // display
               It.Is<ICoding>(cd => cd.Code == "s"), // coding
               null, // concept
               null, // date
               true,  // abstracy
               null // displayLanguage
            ), Times.Once());
        }

        [TestMethod]
        public async void ValidateEmptyString()
        {
            var input = ElementNode.Root("string", value: "");

            var result = await _bindingAssertion.Validate(input, _validationContext);

            Assert.IsFalse(result.Result.IsSuccessful);
            _terminologyService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async void ValidateCodingWithoutCode()
        {
            var input = createCoding("system", null, null);

            var result = await _bindingAssertion.Validate(input, _validationContext);

            Assert.IsFalse(result.Result.IsSuccessful);
            _terminologyService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async void ValidateInvalidCoding()
        {
            SetupTerminologyServiceResult(new Assertions(ResultAssertion.Failure));

            var input = createCoding("http://terminology.hl7.org/CodeSystem/data-absent-reason", "UNKNOWN");
            var result = await _bindingAssertion.Validate(input, _validationContext);

            Assert.IsFalse(result.Result.IsSuccessful);
            _terminologyService.Verify(ts => ts.ValidateCode(
               It.IsAny<string>(), // canonical
               It.IsAny<string>(), // context
               null, // code
               null, // system
               null, // version
               null, // display
               It.Is<ICoding>(cd => cd.Code == "UNKNOWN" && cd.System == "http://terminology.hl7.org/CodeSystem/data-absent-reason"), // coding
               null, // concept
               null, // date
               true,  // abstracy
               null // displayLanguage
            ), Times.Once());
        }
    }
}