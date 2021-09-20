using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{

    public static class PackageRestoreExtensions
    {
        public static async Task<PackageReference> CacheInstall(this PackageContext scope, PackageDependency dependency)
        {
            PackageReference reference;

            if (scope.Server is object)
            {
                reference = await scope.Server.Resolve(dependency);
                if (!reference.Found) return reference;
            }
            else
            {
                reference = await scope.Cache.Resolve(dependency);
                if (!reference.Found) return reference;
            }

            if (await scope.Cache.IsInstalled(reference)) return reference;

            var buffer = await scope.Server.GetPackage(reference);
            if (buffer is null) return PackageReference.None;

            await scope.Cache.Install(reference, buffer);
            scope.Report?.Invoke($"Installed {reference}.");
            return reference;
        }

        public static async Task<PackageClosure> Restore(this PackageContext scope)
        {
            scope.Closure = new PackageClosure(); // reset
            var manifest = await scope.Project.ReadManifest();

            await scope.RestoreManifest(manifest);
            return await scope.SaveClosure();
        }

        public static async Task<PackageClosure> SaveClosure(this PackageContext scope)
        {
            await scope.Project.WriteClosure(scope.Closure);
            return scope.Closure;
        }

        private static async Task RestoreManifest(this PackageContext scope, PackageManifest manifest)
        {
            foreach(PackageDependency dependency in manifest.GetDependencies())
            { 
                await scope.RestoreDependency(dependency);
            }
        }

        private static async Task RestoreDependency(this PackageContext scope, PackageDependency dependency)
        {
            var reference = await scope.CacheInstall(dependency);
            if (reference.Found)
            {
                scope.Closure.Add(reference);
                await scope.RestoreReference(reference);
            }
            else
            {
                scope.Closure.AddMissing(dependency);
            }
        }

        private static async Task RestoreReference(this PackageContext scope, PackageReference reference)
        {
            var manifest = await scope.Cache.ReadManifest(reference);
            if (manifest is object)
            {
                await scope.RestoreManifest(manifest);
            }
        }

    }

}
