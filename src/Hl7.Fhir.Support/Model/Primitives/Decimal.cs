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


    }
}
