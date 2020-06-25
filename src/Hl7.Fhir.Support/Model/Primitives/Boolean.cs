/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;

namespace Hl7.Fhir.Model.Primitives
{
    public static class Boolean
    {
        public static bool Parse(string value) =>
            TryParse(value, out var result) ? result : throw new FormatException("Boolean value is in an invalid format.");

        public static bool TryParse(string representation, out bool value)
        {
            if (representation == "true")
            {
                value = true;
                return true;
            }
            else if (representation == "false")
            {
                value = false;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        // Comparison functions work according to the rules described for CQL, 
        // see https://cql.hl7.org/09-b-cqlreference.html#comparison-operators-4
        // for more details.

        /// <summary>
        /// Compares two booleans according to CQL equality rules.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns>Return true if both arguments are exactly the same boolean value, false otherwise. Returns null if any of the
        /// arguments are null.</returns>
        public static bool IsEqualTo(bool l, bool r) => l == r;

        /// <summary>
        /// Compares two booleans according to CQL equivalence rules.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns>Return true if both arguments are exactly the same boolean value, false otherwise</returns>
        public static bool IsEquivalentTo(bool l, bool r) => l == r;
    }
}
