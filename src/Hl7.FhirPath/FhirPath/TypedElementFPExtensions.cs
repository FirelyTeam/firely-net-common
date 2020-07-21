/* 
 * Copyright (c) 2016, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Utility;
using Hl7.FhirPath;
using Hl7.FhirPath.Sprache;

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

        public static IEnumerable<ITypedElement> Select(this ITypedElement input, string expression, EvaluationContext ctx = null)
        {
            var evaluator = getCompiledExpression(expression);
            return evaluator(input, ctx ?? EvaluationContext.CreateDefault());
        }

        public static object Scalar(this ITypedElement input, string expression, EvaluationContext ctx = null)
        {
            var evaluator = getCompiledExpression(expression);
            return evaluator.Scalar(input, ctx ?? EvaluationContext.CreateDefault());
        }

        public static bool Predicate(this ITypedElement input, string expression, EvaluationContext ctx = null)
        {
            var evaluator = getCompiledExpression(expression);
            return evaluator.Predicate(input, ctx ?? EvaluationContext.CreateDefault());
        }

        public static bool IsBoolean(this ITypedElement input, string expression, bool value, EvaluationContext ctx = null)
        {
            var evaluator = getCompiledExpression(expression);
            return evaluator.IsBoolean(value, input, ctx ?? EvaluationContext.CreateDefault());
        }
    }
}