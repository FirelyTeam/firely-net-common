/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;
using System.Xml;

namespace Hl7.Fhir.Model.Primitives
{
    public abstract class Integer64
    {
        public static long Parse(string value) =>
            TryParse(value, out long result) ? result : throw new FormatException("Integer64 value is in an invalid format.");

        public static bool TryParse(string representation, out long value)
        {
            (var succ, var val) = Any.DoConvert(() => XmlConvert.ToInt64(representation));
            value = val;
            return succ;
        }

        /// <summary>
        /// Compares two 64-bit integers according to CQL equality rules.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns>Return true if both arguments are exactly the same integer value, false otherwise. Returns null if any of the
        /// arguments are null.</returns>
        public static bool IsEqualTo(long l, long r) => l == r;

        /// <summary>
        /// Compares two 64-bit integers according to CQL equivalence rules.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns>Return true if both arguments are exactly the same integer value, false otherwise</returns>
        public static bool IsEquivalentTo(long l, long r) => l == r;
    }
}