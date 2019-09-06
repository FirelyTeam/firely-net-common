using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Schema;
using Hl7.FhirPath;

namespace Hl7.Fhir.Validation.Impl
{
    public class FhirPathAssertion : SimpleAssertion
    {
        private readonly string _key;
        private readonly string _expression;

        public FhirPathAssertion(string key, string expression)
        {
            _key = key;
            _expression = expression;
        }

        protected override string Key => _key;

        protected override object Value => _expression;

        public override Assertions Validate(ITypedElement input, ValidationContext vc)
        {
            var node = input as ScopedNode ?? new ScopedNode(input);

            var context = node.ResourceContext;

            var compiler = vc.fpCompiler;

            var compiledExpression = compiler.Compile(_expression);
            var success = compiledExpression.Predicate(input, new EvaluationContext(context));


            return success ? Assertions.Success : Assertions.Failure;
        }
    }
}
