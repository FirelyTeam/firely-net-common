/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;

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
        public static bool IsEqualTo(this long l, long r) => l == r;          

        // Decimal: values must be equal, trailing zeroes after the decimal are ignored
        public static bool IsEqualTo(this decimal l, decimal r) => l == r;   // aligns with .NET decimal behaviour

        // Boolean: values must be the same
        public static bool IsEqualTo(this bool l, bool r) => l == r;

        // TODO: Move these to the respective classes
        // Date: must be exactly the same
        public static bool IsEqualTo(this PartialDate l, PartialDate r) => throw new NotImplementedException();

        // DateTime: must be exactly the same, respecting the timezone offset (though +00:00 = -00:00 = Z)
        public static bool IsEqualTo(this PartialDateTime l, PartialDateTime r) => throw new NotImplementedException();

        // Time: must be exactly the same
        public static bool IsEqualTo(this PartialTime l, PartialTime r) => throw new NotImplementedException();

        public static bool IsEqualTo(this Coding l, Coding r) => throw new NotImplementedException();

        public static bool IsEqualTo(this Quantity l, Quantity r) => throw new NotImplementedException();
    }
}
