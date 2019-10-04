/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Schema;
using Hl7.FhirPath;
using Hl7.FhirPath.Expressions;

namespace Hl7.Fhir.Validation.Impl
{
    public class FhirPathAssertion : SimpleAssertion
    {
        private readonly string _key;
        private readonly string _expression;
        private readonly IssueSeverity? _severity;
        private readonly bool _bestPractice;

        public FhirPathAssertion(string location, string key, string expression, IssueSeverity? severity, bool bestPractice) : base(location)
        {
            _key = key;
            _expression = expression;
            _severity = severity;
            _bestPractice = bestPractice;
        }

        protected override string Key => _key;

        protected override object Value => _expression;

        public override Assertions Validate(ITypedElement input, ValidationContext vc)
        {
            var node = input as ScopedNode ?? new ScopedNode(input);

            var context = node.ResourceContext;

            if (_bestPractice && vc.ConstraintBestPractices == ValidateBestPractices.Ignore)
                return Assertions.Success;

            var compiler = GetFhirPathCompiler(vc);

            var compiledExpression = compiler.Compile(_expression);
            var success = compiledExpression.Predicate(input, new EvaluationContext(context));

            if (!success)
            {
                var result = Assertions.Failure;
                result += new IssueAssertion((_severity == IssueSeverity.Error) ? 1012 : 1013, input.Location, $"Instance failed constraint '{Key}'", _severity);
                return result;
            }

            return Assertions.Success;
        }

        private FhirPathCompiler GetFhirPathCompiler(ValidationContext context)
        {

            // Use a provided compiler
            if (context?.FhirPathCompiler != null)
                return context.FhirPathCompiler;

            var symbolTable = new SymbolTable();
            symbolTable.AddStandardFP();
            //symbolTable.AddFhirExtensions();

            return new FhirPathCompiler(symbolTable);
        }
    }
}