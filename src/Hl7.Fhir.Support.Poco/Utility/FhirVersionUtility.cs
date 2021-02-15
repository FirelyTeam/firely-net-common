using Hl7.Fhir.Specification;
using System;

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
                //DSTU1 cycle
                "0.01" or "0.05" or "0.06" or "0.11" or "0.0.80" or "0.0.81" or "0.0.82" => FhirRelease.DSTU1,    
                
                //DSTU2 ballot versions
                "0.4.0" or "0.5.0" => FhirRelease.DSTU2,

                //Official DSTU2 versions (and technical corrections) => 1.0.x
                string s when s.StartsWith("1.0") => FhirRelease.DSTU2,

                //STU3 ballot versions
                "1.1.0" or "1.4.0" or "1.6.0" or "1.8.0" => FhirRelease.STU3,
                //Official STU3 versions (and technical corrections) => 3.0.x
                string s when s.StartsWith("3.0") => FhirRelease.STU3,

                //R4 ballot version
                "3.2.0" or "3.3.0" or "3.5.0" or "3.5a.0" or "3.6.0" => FhirRelease.R4,

                //Official R4 versions (and technical corrections) => 4.0.x
                string s when s.StartsWith("4.0") => FhirRelease.R4,

                //R5 ballot versions
                "4.2.0" or "4.4.0" or "4.5.0" => FhirRelease.R5,

                //Official R5 versions (and technical corrections) => 5.0.x
                string s when s.StartsWith("5.0") => FhirRelease.R5,

                _ => throw new NotSupportedException($"Unknown FHIR version {version}")
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
                _ => throw new NotSupportedException($"Unknown FHIR version {fhirRelease}")
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
                _ => throw new NotSupportedException($"Unknown value for the fhirversion MIME-type {fhirMimeVersion}")
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
                _ => throw new NotSupportedException($"Unknown FHIR version {fhirRelease}")
            };
        }

        /// <summary>
        /// Returns the corresponding FHIR release version of the specific Firely SDK core package
        /// </summary>
        /// <param name="packageName">Firely SDK package name </param>
        /// <returns>Official FHIR Release</returns>
        public static FhirRelease FhirReleaseFromCorePackageName(string packageName)
        {
            return packageName switch
            {
                "hl7.fhir.r3.core" => FhirRelease.STU3,
                "hl7.fhir.r4.core" => FhirRelease.R4,
                "hl7.fhir.r5.core" => FhirRelease.R5,
                _ => throw new NotSupportedException($"Unknown package name {packageName}")
            };
        }

        /// <summary>
        /// Returns the corresponding .NET SDK core package of the specific FHIR Release version
        /// </summary>
        /// <param name="fhirRelease">FHIR Release version</param>
        /// <returns>Firely SDK name</returns>
        public static string CorePackageNameFromFhirRelease(FhirRelease fhirRelease)
        {
            return fhirRelease switch
            {
                FhirRelease.STU3 => "hl7.fhir.r3.core",
                FhirRelease.R4 => "hl7.fhir.r4.core",
                FhirRelease.R5 => "hl7.fhir.r5.core",
                _ => throw new NotSupportedException($"Unknown FHIR version {fhirRelease}")
            };
        }

    }
}
