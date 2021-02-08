using Hl7.Fhir.Specification;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hl7.Fhir.Utility
{
    public class FhirReleaseParser
    {
        /// <summary>
        /// Returns a FHIR version as an enum from a version number
        /// </summary>
        /// <param name="version">Fhir Release version number</param>
        /// <returns>Official FHIR Release</returns>
        public static FhirRelease Parse(string version)
        {
            return version switch
            {
                "0.01" => FhirRelease.DSTU1,
                "0.05" => FhirRelease.DSTU1,
                "0.06" => FhirRelease.DSTU1,
                "0.11" => FhirRelease.DSTU1,
                "0.0.80" => FhirRelease.DSTU1,
                "0.0.81" => FhirRelease.DSTU1,
                "0.0.82" => FhirRelease.DSTU1,

                "0.4.0" => FhirRelease.DSTU2,
                "0.5.0" => FhirRelease.DSTU2,
                "1.0.0" => FhirRelease.DSTU2,
                "1.0.1" => FhirRelease.DSTU2,
                "1.0.2" => FhirRelease.DSTU2,

                "1.1.0" => FhirRelease.STU3,
                "1.4.0" => FhirRelease.STU3,
                "1.6.0" => FhirRelease.STU3,
                "1.8.0" => FhirRelease.STU3,
                "3.0.0" => FhirRelease.STU3,
                "3.0.1" => FhirRelease.STU3,
                "3.0.2" => FhirRelease.STU3,

                "3.2.0" => FhirRelease.R4,
                "3.3.0" => FhirRelease.R4,
                "3.5.0" => FhirRelease.R4,
                "3.5a.0" => FhirRelease.R4,
                "3.6.0" => FhirRelease.R4,
                "4.0.0" => FhirRelease.R4,
                "4.0.1" => FhirRelease.R4,

                "4.2.0" => FhirRelease.R5,
                "4.4.0" => FhirRelease.R5,
                "4.5.0" => FhirRelease.R5,
                "5.0.0" => FhirRelease.R5,
                _ => throw new Exception($"Unknown FHIR version {version}")
            };
        }

        /// <summary>
        /// Returns the version number of the latest official FHIR releases
        /// </summary>
        /// <param name="fhirRelease">Official FHIR release</param>
        /// <returns>Latest version number</returns>
        public static string FhirVersionFromRelease(FhirRelease fhirRelease)
        {
            return fhirRelease switch
            {
                FhirRelease.DSTU1 => "0.0.82",
                FhirRelease.DSTU2 => "1.0.2",
                FhirRelease.STU3 => "3.0.2",
                FhirRelease.R4 => "4.0.1",
                FhirRelease.R5 => "4.5.0",
                _ => throw new Exception($"Unknown FHIR version {fhirRelease}")
            };
        }

        /// <summary>
        /// Returns the official FHIR version based on the value of a MIME-Type parameter 'fhirversion'
        /// </summary>
        /// <param name="fhirMimeVersion">'fhirversion' MIME-Type parameter</param>
        /// <returns>Official FHIR Release</returns>
        public static FhirRelease FhirReleaseFromMimeVersion(string fhirMimeVersion)
        {
            // source: https://www.hl7.org/fhir/http.html#version-parameter
            return fhirMimeVersion switch
            {
                "0.0" => FhirRelease.DSTU1,
                "1.0" => FhirRelease.DSTU2,
                "3.0" => FhirRelease.STU3,
                "4.0" => FhirRelease.R4,
                "5.0" => FhirRelease.R5,
                _ => throw new Exception($"Unknown value for the fhirversion MIME-type {fhirMimeVersion}")
            };
        }

        /// <summary>
        ///  Returns the value of the 'fhirversion' MIME-type parameter corresponding to a specific FHIR Version
        /// </summary>
        /// <param name="fhirRelease">Official FHIR release</param>
        /// <returns>Corresponding 'fhirversion' MIME-Type value</returns>
        public static string MimeVersionFromFhirRelease(FhirRelease fhirRelease)
        {
            return fhirRelease switch
            {
                FhirRelease.DSTU1 => "0.0",
                FhirRelease.DSTU2 => "1.0",
                FhirRelease.STU3 => "3.0",
                FhirRelease.R4 => "4.0",
                FhirRelease.R5 => "5.0",
                _ => null
            };
        }
    }
}
