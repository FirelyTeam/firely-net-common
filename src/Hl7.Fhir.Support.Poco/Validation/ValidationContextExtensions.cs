/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using System.ComponentModel.DataAnnotations;

namespace Hl7.Fhir.Validation
{
    public static class ValidationContextExtensions
    {
        private const string RECURSE_ITEM_KEY = "__dotnetapi_recurse__";

        /// <summary>
        /// Alters the ValidationContext to indicate that validation should or should not recurse into nested objects
        /// (i.e. validate members of the validated objects complex members recursively)
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="recursively"></param>
        public static void SetValidateRecursively(this ValidationContext ctx, bool recursively)
        {
            ctx.Items[RECURSE_ITEM_KEY] = recursively;
        }


        /// <summary>
        /// Gets the indication from the ValidationContext whether validation should recurse into nested objects
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static bool ValidateRecursively(this ValidationContext ctx)
        {
            if (ctx.Items.TryGetValue(RECURSE_ITEM_KEY, out object result))
                return result is bool b ? b : false;
            else
                return false;
        }
    }
}
