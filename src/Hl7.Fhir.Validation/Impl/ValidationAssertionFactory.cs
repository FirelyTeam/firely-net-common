using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Schema;
using System;
using System.Collections.Generic;

namespace Hl7.Fhir.Validation.Impl
{
    public class ValidationAssertionFactory : IAssertionFactory
    {
        public IAssertion CreateBindingAssertion(string location, string valueSetUri, BindingAssertion.BindingStrength strength, bool abstractAllowed = true, string description = null)
            => new BindingAssertion(location, valueSetUri, strength, abstractAllowed, description);

        public IAssertion CreateCardinalityAssertion(int? min, string max, string location = null)
            => new CardinalityAssertion(min, max, location);

        public IAssertion CreateChildren(Func<IReadOnlyDictionary<string, IAssertion>> childGenerator)
            => new Children(childGenerator);

        public IAssertion CreateConditionsAssertion()
            => null; // todo

        public void CreateDefaultValue(ITypedElement defaultValue) 
        { }

        public void CreateDocumentation(string label, string shortDescription, string definition, string comment, string requirements, string meaningWhenMissing, string orderMeaning, IEnumerable<string> aliases, IEnumerable<(string system, string systemVersion, string code, string codeDisplay, bool isUserSelected)> codings)
        { }

        public IElementSchema CreateElementSchemaAssertion(Uri id, IEnumerable<IAssertion> assertions = null)
            => new ElementSchema(id, assertions);

        public void CreateExampleValues(IEnumerable<(string label, ITypedElement value)> exampleValues)
        { }

        public IAssertion CreateFhirPathAssertion(string location, string key, string expression, string humanDescription, IssueSeverity? severity, bool bestPractice)
            => new FhirPathAssertion(location, key, expression, humanDescription, severity, bestPractice);

        public IAssertion CreateFixedValueAssertion(string location, ITypedElement fixedValue)
            => new Fixed(location, fixedValue);

        public IAssertion CreateIsModifierAssertion(bool isModifier, string reason = null)
            => null; // todo

        public IAssertion CreateMaxLengthAssertion(string location, int maxLength)
            => new MaxLength(location, maxLength);

        public IAssertion CreateMinMaxValueAssertion(string location, ITypedElement minMaxValue, MinMax minMaxType)
            => new MinMaxValue(location, minMaxValue, minMaxType);

        public IAssertion CreateMustSupportAssertion(bool mustSupport)
            => null;

        public IAssertion CreatePatternAssertion(string location, ITypedElement patternValue)
            => new Pattern(location, patternValue);

        public IAssertion CreateReferenceAssertion(Func<IElementSchema> getSchema, Uri uri)
            => new ReferenceAssertion(getSchema, uri);

        public IAssertion CreateRegexAssertion(string location, string pattern)
            => new RegExAssertion(location, pattern);

        public IAssertion CreateTypesAssertion(IEnumerable<(string code, IEnumerable<string> profileCanonicals)> types)
            => null; // todo
    }
}
