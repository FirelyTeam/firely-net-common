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
using System;

namespace Hl7.Fhir.Validation.Impl
{
    public class FhirPathAssertion : SimpleAssertion
    {
        private readonly string _key;
        private readonly string _humanDescription;
        private readonly string _expression;
        private IssueSeverity? _severity;
        private readonly bool _bestPractice;

        public FhirPathAssertion(string location, string key, string expression, string humanDescription, IssueSeverity? severity, bool bestPractice) : base(location)
        {
            _key = key;
            _humanDescription = humanDescription;
            _expression = expression;
            _severity = severity;
            _bestPractice = bestPractice;
        }

        protected override string Key => _key;

        protected override object Value => _expression;

        public override Assertions Validate(ITypedElement input, ValidationContext vc)
        {
            //if (!vc.Filter.Invoke(this)) return Assertions.Empty + new Trace("Not executed");

            var result = Assertions.Empty;

            var node = input as ScopedNode ?? new ScopedNode(input);
            var context = node.ResourceContext;

            if (_bestPractice)
            {
                switch (vc.ConstraintBestPractices)
                {
                    case ValidateBestPractices.Ignore:
                        return Assertions.Success;
                    case ValidateBestPractices.Enabled:
                        _severity = IssueSeverity.Error;
                        break;
                    case ValidateBestPractices.Disabled:
                        _severity = IssueSeverity.Warning;
                        break;
                    default:
                        break;
                }
            }

            bool success = false;
            var compiler = GetFhirPathCompiler(vc);
            try
            {
                var compiledExpression = compiler.Compile(_expression);
                success = compiledExpression.Predicate(input, new EvaluationContext(context));
            }
            catch (Exception e)
            {
                result += new TraceText($"Evaluation of FhirPath for constraint '{_key}' failed: {e.Message}");
            }

            if (!success)
            {
                result += Assertions.Failure;
                result += new IssueAssertion(_severity == IssueSeverity.Error ? 1012 : 1013, input.Location, $"Instance failed constraint {GetDescription()}", _severity);
                return result;
            }

            return Assertions.Success;
        }

        private string GetDescription()
        {
            var desc = _key;

            if (!string.IsNullOrEmpty(_humanDescription))
                desc += " \"" + _humanDescription + "\"";

            return desc;
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