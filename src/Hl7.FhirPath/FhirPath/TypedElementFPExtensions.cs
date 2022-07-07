/* 
 * Copyright (c) 2016, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

#nullable enable

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Utility;
using System.Collections.Generic;

namespace Hl7.FhirPath
{
    public static class IValueProviderFPExtensions
    {
        public static int MAX_FP_EXPRESSION_CACHE_SIZE = 500;
        private static readonly Cache<string, CompiledExpression> _cache = new Cache<string, CompiledExpression>(expr => Compile(expr), new CacheSettings() { MaxCacheSize = MAX_FP_EXPRESSION_CACHE_SIZE });

        private static CompiledExpression Compile(string expression)
        {
            var compiler = new FhirPathCompiler();
            return compiler.Compile(expression);
        }

        private static CompiledExpression getCompiledExpression(string expression)
        {
            return _cache.GetValue(expression);
        }


        /// <summary>
        /// Evaluates an expression against a given context and returns the result(s)
        /// </summary>
        /// <param name="input">Input on which the expression is being evaluated</param>
        /// <param name="expression">Expression which is to be evaluated</param>
        /// <param name="ctx">Context of the evaluation</param>
        /// <returns>The result(s) of the expression</returns>
        public static IEnumerable<ITypedElement> Select(this ITypedElement input, string expression, EvaluationContext? ctx = null)
        {
            input = input.ToScopedNode();
            var evaluator = getCompiledExpression(expression);
            return evaluator(input, ctx ?? EvaluationContext.CreateDefault());
        }


        /// <summary>
        /// Evaluates an expression against a given context and returns a single result
        /// </summary>
        /// <param name="input">Input on which the expression is being evaluated</param>
        /// <param name="expression">Expression which is to be evaluated</param>
        /// <param name="ctx">Context of the evaluation</param>
        /// <returns>The single result of the expression, and null if the expression returns multiple results</returns>
        public static object? Scalar(this ITypedElement input, string expression, EvaluationContext? ctx = null)
        {
            input = input.ToScopedNode();
            var evaluator = getCompiledExpression(expression);
            return evaluator.Scalar(input, ctx ?? EvaluationContext.CreateDefault());
        }


        /// <summary>
        /// Evaluates an expression and returns true for expression being evaluated as true or empty, otherwise false.
        /// </summary>
        /// <param name="input">Input on which the expression is being evaluated</param>
        /// <param name="expression">Expression which is to be evaluated</param>
        /// <param name="ctx">Context of the evaluation</param>
        /// <returns>True if expression returns true of empty, otheriwse false</returns>
        public static bool Predicate(this ITypedElement input, string expression, EvaluationContext? ctx = null)
        {
            input = input.ToScopedNode();
            var evaluator = getCompiledExpression(expression);
            return evaluator.Predicate(input, ctx ?? EvaluationContext.CreateDefault());
        }


        /// <summary>
        /// Evaluates an expression and returns true for expression being evaluated as true, and false if the expression returns false or empty.
        /// </summary>
        /// <param name="input">Input on which the expression is being evaluated</param>
        /// <param name="expression">Expression which is to be evaluated</param>
        /// <param name="ctx">Context of the evaluation</param>
        /// <returns>True if expression returns true , and false if expression returns empty of false.</returns>
        public static bool IsTrue(this ITypedElement input, string expression, EvaluationContext? ctx = null)
        {
            input = input.ToScopedNode();
            var evaluator = getCompiledExpression(expression);
            return evaluator.IsTrue(input, ctx ?? EvaluationContext.CreateDefault());
        }


        /// <summary>
        ///Evaluates if the result of an expression is equal to a given boolean.
        /// </summary>
        /// <param name="input">Input on which the expression is being evaluated</param>
        /// <param name="value">Boolean that is to be compared to the result of the expression</param>
        /// <param name="expression">Expression which is to be evaluated</param>
        /// <param name="ctx">Context of the evaluation</param>
        /// <returns>True if the result of an expression is equal to a given boolean, otherwise false</returns>
        public static bool IsBoolean(this ITypedElement input, string expression, bool value, EvaluationContext? ctx = null)
        {
            input = input.ToScopedNode();

            var evaluator = getCompiledExpression(expression);
            return evaluator.IsBoolean(value, input, ctx ?? EvaluationContext.CreateDefault());
        }
    }
}

#nullable restore