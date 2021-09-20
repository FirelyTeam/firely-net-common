using SemVer;
using System.Collections.Generic;
using System.Linq;

namespace Firely.Fhir.Packages
{

    public static class VersionsExtensions
    {
        public static Versions ToVersions(this PackageListing listing)
        {
            var versions = new Versions(listing.Versions.Keys);
            return versions;
        }

        public static Versions ToVersions(this IEnumerable<PackageReference> references)
        {
            var list = new Versions(references.Select(r => r.Version));
            return list;
        }

        [System.CLSCompliant(false)]
        public static Version Resolve(this Versions versions, string pattern)
        {
            if (pattern == "latest" || string.IsNullOrEmpty(pattern))
            {
                return versions.Latest();
            }
            var range = new Range(pattern);
            return versions.Resolve(range);
        }

        public static PackageReference Resolve(this Versions versions, PackageDependency dependency)
        {
            var version = versions.Resolve(dependency.Range);
            if (version is null)
            {
                return PackageReference.None;
            }
            return new PackageReference(dependency.Name, version.ToString());
        }

        public static bool Has(this Versions versions, string version)
        {
            var v = new Version(version);
            return versions.Has(v);
        }
    }
}
