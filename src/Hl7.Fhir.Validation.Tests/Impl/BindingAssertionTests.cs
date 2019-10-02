using Hl7.Fhir.ElementModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hl7.Fhir.Validation.Tests.Impl
{
    [TestClass]
    public class BindingAssertionTests
    {
        public void TestValueValidation()
        {
            var binding = new ElementDefinition.ElementDefinitionBindingComponent
            {
                Strength = BindingStrength.Required,
                ValueSet = "http://hl7.org/fhir/ValueSet/data-absent-reason"
            };

            var validator = binding.ToValidatable();
            var vc = new ValidationContext(); // TODO MV Validation { TerminologyService = _termService };
            // Non-bindeable things should succeed
            Element v = new FhirBoolean(true);
            var node = ElementNode.ForPrimitive(true);
            Assert.True(validator.Validate(node, vc).Success);

            v = new Quantity(4.0m, "masked", "http://terminology.hl7.org/CodeSystem/data-absent-reason");  // nonsense, but hey UCUM is not provided with the spec
            node = v.ToTypedElement();
            Assert.True(validator.Validate(node, vc).Success);

            v = new Quantity(4.0m, "maskedx", "http://terminology.hl7.org/CodeSystem/data-absent-reason");  // nonsense, but hey UCUM is not provided with the spec
            node = v.ToTypedElement();
            Assert.False(validator.Validate(node, vc).Success);

            v = new Quantity(4.0m, "kg");  // sorry, UCUM is not provided with the spec - still validate against data-absent-reason
            node = v.ToTypedElement();
            Assert.False(validator.Validate(node, vc).Success);

            v = new FhirString("masked");
            node = v.ToTypedElement();
            Assert.True(validator.Validate(node, vc).Success);

            v = new FhirString("maskedx");
            node = v.ToTypedElement();
            Assert.False(validator.Validate(node, vc).Success);

            var ic = new Coding("http://terminology.hl7.org/CodeSystem/data-absent-reason", "masked");
            var ext = new Extension { Value = ic };
            node = ext.ToTypedElement();
            Assert.True(validator.Validate(node, vc).Success);

            ic.Code = "maskedx";
            node = ext.ToTypedElement();
            Assert.False(validator.Validate(node, vc).Success);
        }

    }
}
