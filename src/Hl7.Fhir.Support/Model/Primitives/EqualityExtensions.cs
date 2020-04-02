/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

namespace Hl7.Fhir.Model.Primitives
{
    /// <summary>
    /// Equality functions for the base set of primitives, based on section 6.1 of the FhirPath spec
    /// </summary>
    /// <remarks>Because PartialDate and PartialTime also accept timezones, the definition below
    /// deviates from the spec, and instead aligns with the logic for PartialDateTime as described
    /// in the FhirPath specification.
    /// </remarks>
    public static class EqualityExtensions
    {
        // String: comparison is based on Unicode values
        public static bool IsEqualTo(this string l, string r) => l == r;

        // Integer: values must be exactly equal
        public static bool IsEqualTo(this int l, int r) => l == r;

        // Long: values must be exactly equal
        public static bool IsEqualTo(this long l, long r) => l == r;

        // Decimal: values must be equal, trailing zeroes after the decimal are ignored
        public static bool IsEqualTo(this decimal l, decimal r) => l == r;   // aligns with .NET decimal behaviour

        // Boolean: values must be the same
        public static bool IsEqualTo(this bool l, bool r) => l == r;
    }
}
