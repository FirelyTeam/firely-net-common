/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Utility;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hl7.Fhir.Introspection
{
    public abstract class VersionedAttribute : Attribute, IFhirVersionDependent
    {
        /// <summary>
        /// First version of FHIR for which this attribute applies, as a semver string.
        /// </summary>
        public string Since
        {
            get => _originalSince;
            set
            {
                _originalSince = value;
                if (!string.IsNullOrEmpty(value))
                {
                    if (SemVersion.TryParse(value, out var semver))
                        SinceVersion = semver;
                    else
                        SinceVersion = null;
                }
            }
        }

        private string _originalSince;

        /// <summary>
        /// First version of FHIR for which this attribute applies, as a semver value.
        /// </summary>
        public SemVersion SinceVersion { get; private set; }
    }

    public abstract class VersionedValidationAttribute : ValidationAttribute, IFhirVersionDependent
    {
        /// <summary>
        /// First version of FHIR for which this attribute applies, as a semver string.
        /// </summary>
        public string Since
        {
            get => _originalSince;
            set
            {
                _originalSince = value;
                if (!string.IsNullOrEmpty(value))
                {
                    if (SemVersion.TryParse(value, out var semver))
                        SinceVersion = semver;
                    else
                        SinceVersion = null;
                }
            }
        }

        private string _originalSince;

        /// <summary>
        /// First version of FHIR for which this attribute applies, as a semver value.
        /// </summary>
        public SemVersion SinceVersion { get; private set; }
    }
}
