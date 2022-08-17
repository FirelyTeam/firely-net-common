/* 
 * Copyright (c) 2016, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

#nullable enable

using Hl7.Fhir.ElementModel;
using System.Collections.Generic;

namespace Hl7.FhirPath
{

    public static class IValueProviderFPExtensions
    {
        private static readonly FhirPathCompilerCache CACHE = new();

        /// <inheritdoc cref="FhirPathCompilerCache.Select(ITypedElement, string, EvaluationContext?)"/>
        public static IEnumerable<ITypedElement> Select(this ITypedElement input, string expression, EvaluationContext? ctx = null)
            => CACHE.Select(input, expression, ctx);

        /// <inheritdoc cref="FhirPathCompilerCache.Scalar(ITypedElement, string, EvaluationContext?)"/>
        public static object? Scalar(this ITypedElement input, string expression, EvaluationContext? ctx = null)
            => CACHE.Scalar(input, expression, ctx);

        /// <inheritdoc cref="FhirPathCompilerCache.Predicate(ITypedElement, string, EvaluationContext?)"/>
        public static bool Predicate(this ITypedElement input, string expression, EvaluationContext? ctx = null)
            => CACHE.Predicate(input, expression, ctx);

        /// <inheritdoc cref="FhirPathCompilerCache.IsTrue(ITypedElement, string, EvaluationContext?)"/>
        public static bool IsTrue(this ITypedElement input, string expression, EvaluationContext? ctx = null)
            => CACHE.IsTrue(input, expression, ctx);

        /// <inheritdoc cref="FhirPathCompilerCache.IsBoolean(ITypedElement, string, bool, EvaluationContext?)"/>
        public static bool IsBoolean(this ITypedElement input, string expression, bool value, EvaluationContext? ctx = null)
            => CACHE.IsBoolean(input, expression, value, ctx);
    }
}

#nullable restore