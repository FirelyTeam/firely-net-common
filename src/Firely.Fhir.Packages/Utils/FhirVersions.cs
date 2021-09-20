using Hl7.Fhir.Specification;
using System;

namespace Firely.Fhir.Packages
{
#if BUILDFHIRRELEASEENUM
    // SDK does not have FhirRelease before 2.0
    public enum FhirRelease
    {
        DSTU1,
        DSTU2,
        STU3,
        R4,
        R5,
        R4B
    }
#endif

    public static class FhirVersions
    {
        [Obsolete("With the introduction of release 4b, integer-numbered releases are no longer useable.")]
        public static int ParseToInt(string version)
        {
            return version switch
            {
                "0.01" => 1,
                "0.05" => 1,
                "0.06" => 1,
                "0.11" => 1,
                "0.0.80" => 1,
                "0.0.81" => 1,
                "0.0.82" => 1,

                "0.4.0" => 2,
                "0.5.0" => 2,
                "1.0.0" => 2,
                "1.0.1" => 2,
                "1.0.2" => 2,

                "1.1.0" => 3,
                "1.4.0" => 3,
                "1.6.0" => 3,
                "1.8.0" => 3,
                "3.0.0" => 3,
                "3.0.1" => 3,
                "3.0.2" => 3,

                "3.2.0" => 4,
                "3.3.0" => 4,
                "3.5.0" => 4,
                "3.6.0" => 4,
                "4.0.0" => 4,
                "4.0.1" => 4,
                "4.5.0" => 5,
                _ => -1
            };
        }

        

        public static FhirRelease? TryParse(string version)
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
                "3.6.0" => FhirRelease.R4,
                "4.0.0" => FhirRelease.R4,
                "4.0.1" => FhirRelease.R4,

                "4.5.0" => FhirRelease.R5,
                _ => null
            };
        }

        public static FhirRelease Parse(string version)
        {
            var release = TryParse(version);
            if (release is null) throw new ArgumentException($"{version} is not a known FHIR version.", nameof(version));
            else return release.Value;
        }

        [Obsolete("With the introduction of release 4b, integer-numbered releases are no longer useable.")]
        public static string GetFhirVersion(int fhirRelease)
        {
            return fhirRelease switch
            {
                1 => "0.0.82",
                2 => "1.0.2",
                3 => "3.0.1",
                4 => "4.0.0",
                5 => "4.5.0",
                _ => throw new Exception($"Unknown FHIR version {fhirRelease}")
            };
        }

        public static string FhirVersionFromRelease(FhirRelease fhirRelease)
        {
            return fhirRelease switch
            {
                FhirRelease.DSTU1 => "0.0.82",
                FhirRelease.DSTU2 => "1.0.2",
                FhirRelease.STU3 => "3.0.1",
                FhirRelease.R4 => "4.0.0",
                FhirRelease.R5 => "4.5.0",
                _ => throw new Exception($"Unknown FHIR version {fhirRelease}")
            };
        }
    }
}
