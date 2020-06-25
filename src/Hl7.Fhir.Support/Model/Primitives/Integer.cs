/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;
using System.Xml;

namespace Hl7.Fhir.Model.Primitives
{
    public static class Integer
    {
        public static int Parse(string value) =>
            TryParse(value, out int result) ? result : throw new FormatException("Integer value is in an invalid format.");

        public static bool TryParse(string representation, out int value)
        {
            (var succ, var val) = Any.DoConvert(() => XmlConvert.ToInt32(representation));
            value = val;
            return succ;
        }


        /// <summary>
        /// Compares two integers according to CQL equality rules.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns>Return true if both arguments are exactly the same integer value, false otherwise. Returns null if any of the
        /// arguments are null.</returns>
        public static bool IsEqualTo(int l, int r) => l == r;          

        /// <summary>
        /// Compares two integers according to CQL equivalence rules.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns>Return true if both arguments are exactly the same integer value, false otherwise</returns>
        public static bool IsEquivalentTo(int l, int r) => l == r;
    }
}
