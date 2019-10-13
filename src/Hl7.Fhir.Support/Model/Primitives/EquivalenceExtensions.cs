/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;
using System.Globalization;

namespace Hl7.Fhir.Model.Primitives
{
    /// <summary>
    /// Equality functions for the base set of primitives, based on section 6.1 of the FhirPath spec
    /// </summary>
    /// <remarks>Because PartialDate and PartialTime also accept timezones, the definition below
    /// deviates from the spec, and instead aligns with the logic for PartialDateTime as described
    /// in the FhirPath specification.
    /// </remarks>
    public static class EquivalenceExtensions
    {
        // String: comparison is based on Unicode values
        public static bool IsEquivalentTo(this string l, string r) => l == r;

        // Integer: values must be exactly equal
        public static bool IsEquivalentTo(this long l, long r) => l == r;          

        public static bool IsEquivalentTo(this bool l, bool r) => l == r;   // aligns with .NET decimal behaviour

        // Decimal: values must be equal, trailing zeroes after the decimal are ignored
        public static bool IsEquivalentTo(this decimal a, decimal b) => a == b;   // aligns with .NET decimal behaviour
    }
}
