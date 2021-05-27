/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

namespace Hl7.Fhir.Specification
{
    /// <summary>
    /// The major FHIR publication releases.
    /// </summary>
    /// <remarks>Note: this is set is ordered, so "older release" is less than "newer release".</remarks>
    public enum FhirRelease
    {
        DSTU1,
        DSTU2,
        STU3,
        R4,
        R4B,
        R5,
    }
}
