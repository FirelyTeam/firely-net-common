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

namespace Hl7.Fhir.Model.Primitives
{
    public abstract class Decimal
    {
        // private static readonly string[] FORBIDDEN_DECIMAL_PREFIXES = new[] { "+", ".", "00" };
        // [20190819] EK Consolidated this syntax with CQL and FhirPath, which will allow leading zeroes 
        private static readonly string[] FORBIDDEN_DECIMAL_PREFIXES = new[] { "+", "." };

        public static decimal Parse(string value) =>
            TryParse(value, out decimal result) ? result : throw new FormatException("Decimal value is in an invalid format.");

        public static bool TryParse(string representation, out decimal value)
        {
            if (FORBIDDEN_DECIMAL_PREFIXES.Any(prefix => representation.StartsWith(prefix)) || representation.EndsWith("."))
            {
                value = default;
                return false;
            }
            else
            {
                (var succ, var val) = Any.DoConvert(() => decimal.Parse(representation, NumberStyles.AllowDecimalPoint |
                       NumberStyles.AllowExponent |
                       NumberStyles.AllowLeadingSign,
                       CultureInfo.InvariantCulture));
                value = val;
                return succ;
            }
        }


        // Comparison functions work according to the rules described for CQL, 
        // see https://cql.hl7.org/09-b-cqlreference.html#comparison-operators-4
        // for more details.

        /// <summary>
        /// Compares two decimals according to CQL equality rules.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns>Return true if the arguments are equal, trailing zeroes after the decimal are ignored. Returns null if any of the
        /// arguments are null.</returns>
        public static bool? IsEqualTo(decimal? l, decimal? r) =>
            (l == null || r == null)
                ? null
                : (bool?)(l == r);

        /// <summary>
        /// Compares two decimals according to CQL equivalence rules.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns>Return true if both arguments are equal, comparison is done on values rounded to the precision of the
        /// least precise operand. Trailing zeroes after the decimal are ignored in determining precision.</returns>
        public static bool IsEquivalentTo(decimal? l, decimal? r)
        {
            if (l == null && r == null) return true;
            if (l == null || r == null) return false;

            // The CQL and FhirPath spec talk about 'precision' (number of digits), but might mean 'scale' 
            // (number of decimals). Since the first has no native support on .NET, I'll be sloppy and
            // assume scale is meant.
            var roundPrec = Math.Min(Scale(l.Value, true), Scale(r.Value, true));
            var lr = Math.Round(l.Value, roundPrec);
            var rr = Math.Round(r.Value, roundPrec);
            
            return lr == rr;
        }

        public static int Scale(decimal d, bool ignoreTrailingZeroes)
        {
            var sr = d.ToString(CultureInfo.InvariantCulture);
            var pointPos = sr.IndexOf('.');
            if (pointPos == -1) return 0;
            
            if (ignoreTrailingZeroes) sr = sr.TrimEnd('0');
            
            return pointPos == -1 ? 0 : sr.Length - pointPos - 1;   // -1 for the decimal sign
        }
    }
}
