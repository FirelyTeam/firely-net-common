using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public static class IPackageCacheExtensions
    {
        public static async Task<PackageManifest> ReadManifest(this IPackageCache cache, string name, string version)
        {
            var reference = new PackageReference(name, version);
            return await cache.ReadManifest(reference);
        }

        public static async Task<CanonicalIndex> ReadCanonicalIndex(this IPackageCache cache, string name, string version)
        {
            var reference = new PackageReference(name, version);
            return await cache.GetCanonicalIndex(reference);
        }

        public static IEnumerable<PackageReference> WithName(this IEnumerable<PackageReference> refs, string name)
        {
            return refs.Where(r => string.Compare(r.Name, name, ignoreCase: true) == 0);
        }

        //public static IEnumerable<PackageReference> GetInstalledVersions(this IPackageCache cache, string pkgname)
        //{
        //    return cache.GetPackageReferences().WithName(pkgname);
        //}

        public static async Task<PackageReference> InstallFromFile(this IPackageCache cache, string path)
        {
            var manifest = Packaging.ExtractManifestFromPackageFile(path);
            var reference = manifest.GetPackageReference();
            var buffer = File.ReadAllBytes(path);
            
            await cache.Install(reference, buffer);

            return reference; 
        }

        public static async Task<string> GetFileContent(this IPackageCache cache, PackageFileReference reference)
        {
            return await cache.GetFileContent(reference.Package, reference.FileName);
        }

        public static async Task<string> ReadPackageFhirVersion(this IPackageCache cache, PackageReference reference)
        {
            var m = await cache.ReadManifest(reference);
            var fhirVersion = m.GetFhirVersion();
            return fhirVersion;
        } 

    }
}

