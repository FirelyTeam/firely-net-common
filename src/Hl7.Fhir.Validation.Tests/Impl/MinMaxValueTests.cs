using FluentAssertions;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model;
using Hl7.Fhir.Validation.Impl;
using Hl7.Fhir.Validation.Impl.Tests;
using Hl7.Fhir.Validation.Schema;
using Hl7.Fhir.Validation.Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Tests.Impl
{
    internal class MinValueAssertionData : SimpleAssertionDataAttribute
    {
        private readonly IValidatable _validatableMinValue = new MinMaxValue(PrimitiveTypeExtensions.ToTypedElement<Integer, int?>(4), MinMax.MinValue);
        private readonly IValidatable _validatableMaxValue = new MinMaxValue(PrimitiveTypeExtensions.ToTypedElement<Date, string>("1905-08-23"), MinMax.MaxValue);

        public override IEnumerable<object[]> GetData()
        {
            yield return new object[]
            {
                _validatableMinValue,
                PrimitiveTypeExtensions.ToTypedElement<FhirString, string>("a string"),
                false, Issue.CONTENT_ELEMENT_PRIMITIVE_VALUE_NOT_COMPARABLE, "CompareWithOtherPrimitive"
            };
            yield return new object[]
            {
                _validatableMinValue,
                PrimitiveTypeExtensions.ToTypedElement<Integer, int?>(3),
                false, Issue.CONTENT_ELEMENT_PRIMITIVE_VALUE_TOO_SMALL, "LessThan"
            };
            yield return new object[]
            {
                _validatableMinValue,
                PrimitiveTypeExtensions.ToTypedElement<Integer, int?>(4),
                true, null, "Equals"
            };
            yield return new object[]
            {
                _validatableMinValue,
                PrimitiveTypeExtensions.ToTypedElement<Integer, int?>(5),
                true, null, "GreatThan"
            };

            yield return new object[]
            {
                _validatableMaxValue,
                PrimitiveTypeExtensions.ToTypedElement<Integer, int?>(2),
                false, Issue.CONTENT_ELEMENT_PRIMITIVE_VALUE_NOT_COMPARABLE, "CompareWithOtherPrimitive"
            };
            yield return new object[]
            {
                _validatableMaxValue,
                PrimitiveTypeExtensions.ToTypedElement<Date, string>("1905-01-01"),
                true, null, "LessThan"
            };
            yield return new object[]
            {
                _validatableMaxValue,
                PrimitiveTypeExtensions.ToTypedElement<Date, string>("1905"),
                true, null, "PartialEquals"
            };
            yield return new object[]
            {
                _validatableMaxValue,
                PrimitiveTypeExtensions.ToTypedElement<Date, string>("1905-08-23"),
                true, null, "Equals"
            };
            yield return new object[]
            {
                _validatableMaxValue,
                PrimitiveTypeExtensions.ToTypedElement<Date, string>("1905-12-31"),
                false, Issue.CONTENT_ELEMENT_PRIMITIVE_VALUE_TOO_LARGE, "GreaterThan"
            };
            yield return new object[]
            {
                _validatableMaxValue,
                PrimitiveTypeExtensions.ToTypedElement<Date, string>("1906"),
                false, Issue.CONTENT_ELEMENT_PRIMITIVE_VALUE_TOO_LARGE, "PartialGreaterThan"
            };
        }
    }

    [TestClass]
    public class MinMaxValueTests : SimpleAssertionTests
    {
        [TestMethod]
        public void InvalidConstructors()
        {
            Action action = () => new MinMaxValue(null, MinMax.MaxValue);
            action.Should().Throw<ArgumentNullException>();

            var humanNameValue = ElementNode.Root("HumanName");
            humanNameValue.Add("family", "Brown", "string");

            action = () => new MinMaxValue(humanNameValue, MinMax.MaxValue);
            action.Should().Throw<IncorrectElementDefinitionException>();
        }

        [TestMethod]
        public void CorrectConstructor()
        {
            var assertion = new MinMaxValue(PrimitiveTypeExtensions.ToTypedElement<Integer, int?>(4), MinMax.MaxValue);

            assertion.Should().NotBeNull();
            assertion.Key.Should().Be("maxValue[x]");
            assertion.Value.Should().BeAssignableTo<ITypedElement>();
        }

        [DataTestMethod]
        [MinValueAssertionData]
        public override Task SimpleAssertionTestcases(SimpleAssertion assertion, ITypedElement input, bool expectedResult, Issue expectedIssue, string failureMessage)
            => base.SimpleAssertionTestcases(assertion, input, expectedResult, expectedIssue, failureMessage);
    }
}