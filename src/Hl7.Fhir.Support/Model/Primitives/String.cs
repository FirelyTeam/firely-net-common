/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace Hl7.Fhir.Model.Primitives
{
    public abstract class String
    {
        public static string Parse(string value) =>
            TryParse(value, out string result) ? result : throw new FormatException("String value is in an invalid format.");

        public static bool TryParse(string representation, out string value)
        {
            value = representation;   // a bit obvious
            return true;
        }

        // Comparison functions work according to the rules described for CQL, 
        // see https://cql.hl7.org/09-b-cqlreference.html#comparison-operators-4
        // for more details.

        /// <summary>
        /// Compares two strings according to CQL equality rules.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns>Returns true if both are the same based on the Unicode values for the individual 
        /// characters in the strings.</returns>
        public static bool IsEqualTo(string l, string r) => string.CompareOrdinal(l,r) == 0;

        /// <summary>
        /// Compares two strings according to CQL equivalence rules.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns>Return true if the strings are the same value while ignoring case and locale, 
        /// and normalizing whitespace. Normalizing whitespace means that all whitespace characters are 
        /// treated as equivalent, with whitespace characters as defined in the whitespace lexical category.
        /// </returns>
        public static bool IsEquivalentTo(string l, string r) =>
#if !NETSTANDARD1_1
            string.Compare(normalizeWS(l), normalizeWS(r), CultureInfo.InvariantCulture,
                CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) == 0;
#else
            string.Compare(normalizeWS(l), normalizeWS(r), StringComparison.OrdinalIgnoreCase) == 0;
#endif

        private static string normalizeWS(string data)
        {
            if (data == null) return null;

            var dataAsChars = data.ToCharArray();
            for (var ix = 0; ix < dataAsChars.Length; ix++)
            {
                if (char.IsWhiteSpace(dataAsChars[ix]))
                    dataAsChars[ix] = ' ';               
            }

            return new string(dataAsChars);
        }
    }
}
