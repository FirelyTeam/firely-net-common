using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public static class PackageServerExtensions
    {
        public static async ValueTask<Versions> GetVersions(this IPackageServer server, PackageDependency dependency)
        {
            return await server.GetVersions(dependency.Name);
        }

        public static async ValueTask<PackageReference> Resolve(this IPackageServer server, PackageDependency dependency)
        {
            var versions = await server.GetVersions(dependency.Name);
            var version = versions.Resolve(dependency.Range)?.ToString(); //null => NotFound
            if (version is null) return PackageReference.None;

            return new PackageReference(dependency.Name, version);
        }

        public static async ValueTask<PackageReference> GetLatest(this IPackageServer server, string name)
        {
            var versions = await server.GetVersions(name);
            var version = versions.Latest()?.ToString();
            return new PackageReference(name, version);
        }

        public async static ValueTask<bool> HasMatch(this IPackageServer server, PackageDependency dependency)
        {
            var reference = await server.Resolve(dependency);
            return await Task.FromResult(reference.Found);
        }
    }
}

