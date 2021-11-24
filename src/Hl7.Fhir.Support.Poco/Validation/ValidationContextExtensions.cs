/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using System.ComponentModel.DataAnnotations;

#nullable enable

namespace Hl7.Fhir.Validation
{
    /// <summary>
    /// Extension methods on <see cref="ValidationContext" /> to support POCO validation.
    /// </summary>
    public static class ValidationContextExtensions
    {
        private const string RECURSE_ITEM_KEY = "__dotnetapi_recurse__";
        private const string NARRATIVE_VALIDATION_KIND_ITEM_KEY = "__dotnetapi_narrative_validation_kind__";

        /// <summary>
        /// Alters the ValidationContext to indicate that validation should or should not recurse into nested objects
        /// (i.e. validate members of the validated objects complex members recursively)
        /// </summary>
        public static ValidationContext SetValidateRecursively(this ValidationContext ctx, bool recursively)
        {
            ctx.Items[RECURSE_ITEM_KEY] = recursively;
            return ctx;
        }

        /// <summary>
        /// Gets the indication from the ValidationContext whether validation should recurse into nested objects
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static bool ValidateRecursively(this ValidationContext ctx) =>
            ctx.Items.TryGetValue(RECURSE_ITEM_KEY, out var result) && result is bool b && b;

        /// <summary>
        /// Alters the ValidationContext to indicate the kind of narrative validation the
        /// <see cref="NarrativeXhtmlPatternAttribute"/> should perform.
        /// </summary>
        public static ValidationContext SetNarrativeValidationKind(this ValidationContext ctx, NarrativeValidationKind kind)
        {
            ctx.Items[NARRATIVE_VALIDATION_KIND_ITEM_KEY] = kind;
            return ctx;
        }

        /// <summary>
        /// Gets the kind of narrative validation the <see cref="NarrativeXhtmlPatternAttribute"/> should perform
        /// from the ValidationContext.
        /// </summary>
        public static NarrativeValidationKind GetNarrativeValidationKind(this ValidationContext ctx) =>
            ctx.Items.TryGetValue(NARRATIVE_VALIDATION_KIND_ITEM_KEY, out var result) && result is NarrativeValidationKind k ?
                    k : NarrativeValidationKind.FhirXhtml;
    }
}

#nullable restore