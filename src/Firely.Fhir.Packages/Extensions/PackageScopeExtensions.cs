using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public static class PackageScopeExtensions
    {
        public static async Task<string> GetFileContentByCanonical(this PackageContext scope, string uri, string version = null, bool resolveBestCandidate = false)
        {
            var reference = resolveBestCandidate
                ? scope.Index.ResolveBestCandidateByCanonical(uri, version)
                : scope.Index.ResolveCanonical(uri, version);

            return reference is not null ? await scope.GetFileContent(reference) : null;
        }

        public static async Task<PackageReference> Install(this PackageContext scope, string name, string range)
        {
            var dependency = new PackageDependency(name, range);

            return await scope.CacheInstall(dependency);
        }

        public static PackageFileReference GetFileReferenceByCanonical(this PackageContext scope, string uri, string version = null, bool resolveBestCandidate = false)
        {
            return resolveBestCandidate
                ? scope.Index.ResolveBestCandidateByCanonical(uri, version)
                : scope.Index.ResolveCanonical(uri, version);
        }

        public static async Task<string> GetFileContent(this PackageContext scope, PackageFileReference reference)
        {
            return !reference.Package.Found
                ? await scope.Project.GetFileContent(reference.FilePath)
                : await scope.Cache.GetFileContent(reference);
        }

        public static IEnumerable<string> ReadAllFiles(this PackageContext scope)
        {
            foreach (var reference in scope.Index)
            {
                var content = scope.GetFileContent(reference).Result;
                yield return content;
            }
        }

        public static IEnumerable<string> GetContentsForRange(this PackageContext scope, IEnumerable<PackageFileReference> references)
        {
            foreach (var item in references)
            {
                var content = scope.GetFileContent(item).Result;
                yield return content;
            }
        }

        public static async Task EnsureManifest(this PackageContext scope, string name, string fhirVersion)
        {
            var manifest = await scope.Project.ReadManifest();
            manifest ??= ManifestFile.Create(name, fhirVersion);
            await scope.Project.WriteManifest(manifest);
        }

        [Obsolete("With the introduction of release 4b, integer-numbered releases are no longer useable.")]
        public static async Task EnsureManifest(this PackageContext scope, string name, int fhirRelease)
        {
            var fhirversion = FhirVersions.GetFhirVersion(fhirRelease);
            var manifest = await scope.Project.ReadManifest();
            manifest ??= ManifestFile.Create(name, fhirversion);
            await scope.Project.WriteManifest(manifest);
        }



        public class InstallResult
        {
            public PackageClosure Closure;
            public PackageReference Reference;
        }

        public static async Task<InstallResult> Install(this PackageContext scope, PackageDependency dependency)
        {
            var reference = await scope.CacheInstall(dependency);
            if (reference.NotFound) throw new Exception($"Package '{dependency}' was not found.");

            if (!await scope.Project.HasManifest())
            {
                var fhirVersion = await scope.Cache.ReadPackageFhirVersion(reference);
                await scope.EnsureManifest("project", fhirVersion);
            }

            await scope.Project.AddDependency(dependency);

            var closure = await scope.Restore();
            return new InstallResult { Closure = closure, Reference = reference };
        }

        private static PackageFileReference getFileReference(this PackageContext scope, string resourceType, string id)
        {
            return scope.Index.Where(i => i.ResourceType == resourceType && i.Id == id).FirstOrDefault();

        }

        public static async Task<string> GetFileContentById(this PackageContext scope, string resourceType, string id)
        {
            var reference = scope.getFileReference(resourceType, id);
            if (reference is null) return null;

            var content = await scope.GetFileContent(reference);
            return content;
        }
        public static IEnumerable<string> GetFileNames(this PackageContext scope)
        {
            return scope.Index.Select(i => i.FileName);
        }

        public static async Task<string> GetFileContentByFileName(this PackageContext scope, string fileName)
        {
            var reference = scope.Index.Where(i => i.FileName == fileName).FirstOrDefault();
            if (reference is null) return null;

            var content = await scope.GetFileContent(reference);
            return content;
        }
        public static async Task<string> GetFileContentByFilePath(this PackageContext scope, string filePath)
        {
            var reference = scope.Index.Where(i => i.FilePath == filePath).FirstOrDefault();
            if (reference is null) return null;

            var content = await scope.GetFileContent(reference);
            return content;
        }

    }
}
