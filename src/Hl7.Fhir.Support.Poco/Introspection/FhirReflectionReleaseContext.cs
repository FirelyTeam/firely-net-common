using Hl7.Fhir.Specification;
using System;
using System.Reflection;

namespace Hl7.Fhir.Introspection
{
    public class FhirReflectionReleaseContext
    {
        public Type PocoType { get; }

        public FhirRelease Since { get; }

        public static bool TryBuildFromTypeName(Assembly a, string fhirTypeName, out FhirReflectionReleaseContext rc)
        {
            if (a.TryGetFhirTypeByName(fhirTypeName, out var t))
            {
                rc = new FhirReflectionReleaseContext(t);
                return true;
            }
            else
            {
                rc = default;
                return false;
            }
        }

        public FhirReflectionReleaseContext(Type pocoType)
        {
            PocoType = pocoType;
            Since = pocoType.Assembly.GetCustomAttribute<FhirModelAssemblyAttribute>()!.Since;
        }

        public FhirReflectionReleaseContext(Type pocoType, FhirRelease since)
        {
            PocoType = pocoType;
            Since = since;
        }

        public FhirReflectionReleaseContext ForOtherType(Type t) => new FhirReflectionReleaseContext(t, Since);
    }
}