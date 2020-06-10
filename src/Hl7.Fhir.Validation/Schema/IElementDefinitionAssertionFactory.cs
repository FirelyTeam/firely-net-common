using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Impl;
using Hl7.Fhir.Validation.Support;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Hl7.Fhir.Validation.Impl.SliceAssertion;

namespace Hl7.Fhir.Validation.Schema
{
    public interface IElementDefinitionAssertionFactory
    {
        IElementSchema CreateElementSchemaAssertion(Uri id, IEnumerable<IAssertion> assertions = null);

        IAssertion CreateBindingAssertion(string valueSetUri, BindingAssertion.BindingStrength strength, bool abstractAllowed = true, string description = null);

        IAssertion CreateCardinalityAssertion(int? min, string max, string location = null);

        IAssertion CreateChildren(Func<IReadOnlyDictionary<string, IAssertion>> childGenerator, bool allowAdditionalChildren);

        IAssertion CreateConditionsAssertion();

        void CreateDefaultValue(ITypedElement defaultValue);

        void CreateDocumentation(string label, string shortDescription, string definition, string comment, string requirements, string meaningWhenMissing, string orderMeaning, IEnumerable<string> aliases, IEnumerable<(string system, string systemVersion, string code, string codeDisplay, bool isUserSelected)> codings);

        void CreateExampleValues(IEnumerable<(string label, ITypedElement value)> exampleValues);

        IAssertion CreateFhirPathAssertion(string key, string expression, string humanDescription, IssueSeverity? severity, bool bestPractice);

        IAssertion CreateFixedValueAssertion(ITypedElement fixedValue);

        IAssertion CreateIsModifierAssertion(bool isModifier, string reason = null);

        IAssertion CreateMaxLengthAssertion(int maxLength);

        IAssertion CreateMinMaxValueAssertion(ITypedElement minMaxValue, MinMax minMaxType);

        IAssertion CreateMustSupportAssertion(bool mustSupport);

        IAssertion CreatePatternAssertion(ITypedElement patternValue);

        IAssertion CreateReferenceAssertion(Func<Uri, Task<IElementSchema>> getSchema, Uri uri, IEnumerable<AggregationMode?> aggregations = null);

        IAssertion CreateExtensionAssertion(Func<Uri, Task<IElementSchema>> getSchema, Uri uri);

        IAssertion CreateRegexAssertion(string pattern);

        IAssertion CreateTypesAssertion(IEnumerable<(string code, IEnumerable<string> profileCanonicals)> types);

        IAssertion CreateSliceAssertion(bool ordered, IAssertion @default, IEnumerable<Slice> slices);

        SliceAssertion.Slice CreateSlice(string name, IAssertion condition, IAssertion assertion);
    }
}
