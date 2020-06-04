/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Utility;
using Hl7.Fhir.Validation.Schema;
using Hl7.FhirPath;
using Hl7.FhirPath.Expressions;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Impl
{
    public class FhirPathAssertion : SimpleAssertion
    {
        private readonly string _key;
        private readonly string _humanDescription;
        private readonly string _expression;
        private IssueSeverity? _severity;
        private readonly bool _bestPractice;
        private readonly CompiledExpression _defaultCompiledExpression;

        public FhirPathAssertion(string key, string expression) : this(null, key, expression, null) { }


        public FhirPathAssertion(string location, string key, string expression, string humanDescription, IssueSeverity? severity = IssueSeverity.Error, bool bestPractice = false) : base(location)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _expression = expression ?? throw new ArgumentNullException(nameof(expression));
            _severity = severity ?? throw new ArgumentNullException(nameof(severity));
            _humanDescription = humanDescription;
            _bestPractice = bestPractice;
            _defaultCompiledExpression = GetDefaultCompiledExpression(expression);
        }

        public override string Key => _key;

        public override object Value => _expression;

        public override JToken ToJson()
        {
            var props = new JObject(
                     new JProperty("key", _key),
                     new JProperty("expression", _expression),
                     new JProperty("severity", _severity?.GetLiteral()),
                     new JProperty("bestPractice", _bestPractice)
                    );
            if (_humanDescription != null)
                props.Add(new JProperty("humanDescription", _humanDescription));
            return new JProperty($"fhirPath-{_key}", props);
        }

        public override Task<Assertions> Validate(ITypedElement input, ValidationContext vc)
        {
            var result = Assertions.Empty;

            var node = input as ScopedNode ?? new ScopedNode(input);
            var context = node.ResourceContext;

            if (_bestPractice)
            {
                switch (vc.ConstraintBestPractices)
                {
                    case ValidateBestPractices.Ignore:
                        return Task.FromResult(Assertions.Success);
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
            try
            {
                success = Predicate(input, new EvaluationContext(context), vc);
            }
            catch (Exception e)
            {
                result += new TraceText($"Evaluation of FhirPath for constraint '{_key}' failed: {e.Message}");
            }

            if (!success)
            {
                result += ResultAssertion.CreateFailure(new IssueAssertion(_severity == IssueSeverity.Error ? 1012 : 1013, input.Location, $"Instance failed constraint {GetDescription()}", _severity));
                return Task.FromResult(result);
            }

            return Task.FromResult(Assertions.Success);
        }

        private string GetDescription()
        {
            var desc = _key;

            if (!string.IsNullOrEmpty(_humanDescription))
                desc += " \"" + _humanDescription + "\"";

            return desc;
        }

        private CompiledExpression GetDefaultCompiledExpression(string expression)
        {
            var symbolTable = new SymbolTable();
            symbolTable.AddStandardFP();
            symbolTable.AddFhirExtensions();

            try
            {
                var compiler = new FhirPathCompiler(symbolTable);
                return compiler.Compile(expression);
            }
            catch (Exception ex)
            {
                throw new IncorrectElementDefinitionException($"Error during compilation expression ({expression})", ex);
            }
        }

        private bool Predicate(ITypedElement input, EvaluationContext context, ValidationContext vc)
        {
            var compiledExpression = (vc?.FhirPathCompiler == null)
                ? _defaultCompiledExpression : vc?.FhirPathCompiler.Compile(_expression);

            return compiledExpression.Predicate(input, context);
        }

    }

    internal static class FPExtensions
    {
        public static SymbolTable AddFhirExtensions(this SymbolTable t)
        {
            t.Add("hasValue", (ITypedElement f) => HasValue(f), doNullProp: false);

            // Pre-normative this function was called htmlchecks, normative is htmlChecks
            // lets keep both to keep everyone happy.
            t.Add("htmlchecks", (ITypedElement f) => HtmlChecks(f), doNullProp: false);
            t.Add("htmlChecks", (ITypedElement f) => HtmlChecks(f), doNullProp: false);

            return t;
        }

        public static bool HasValue(ITypedElement focus)
        {
            if (focus == null)
                return false;
            if (focus.Value == null)
                return false;
            return true;
        }

        /// <summary>
        /// Check if the node has a value, and not just extensions.
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static bool HtmlChecks(ITypedElement focus)
        {
            if (focus == null)
                return false;
            if (focus.Value == null)
                return false;
            // Perform the checking of the content for valid html content
            var html = focus.Value.ToString();
            // TODO: Perform the checking
            return true;
        }
    }
}