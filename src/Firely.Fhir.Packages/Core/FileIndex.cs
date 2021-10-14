using Hl7.Fhir.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public class FileIndex : List<PackageFileReference>
    {

        public FileIndex()
        {
        }

        public PackageFileReference ResolveCanonical(string canonical, string version = null)
        {
            var candidates = !string.IsNullOrEmpty(version)
                ? this.Where(r => r.Canonical == canonical && r.Version == version)
                : this.Where(r => r.Canonical == canonical);

            return candidates.Count() > 1 ? candidates.ResolveFromMultipleCandidates(canonical) : candidates.SingleOrDefault();
        }

        public void Add(PackageReference package, ResourceMetadata metadata)
        {
            var reference = new PackageFileReference() { Package = package };
            metadata.CopyTo(reference);
            Add(reference);
        }
    }

    public static class FileIndexExtensions
    {
        internal static async Task Index(this FileIndex index, IPackageCache cache, PackageClosure closure)
        {
            foreach (var reference in closure.References)
            {
                await index.Index(cache, reference);
            }
        }

        internal static PackageFileReference ResolveFromMultipleCandidates(this IEnumerable<PackageFileReference> candidates, string canonical)
        {
            candidates = candidates.Where(c => c.HasSnapshot || c.HasExpansion);
            if (candidates.Count() == 1)
                return candidates.First();
            else
                throw new InvalidOperationException("Found multiple conflicting conformance resources with the same canonical url identifier.");
        }

        internal static async Task Index(this FileIndex index, IPackageCache cache, PackageReference reference)
        {
            var idx = await cache.GetCanonicalIndex(reference);

            index.Add(reference, idx);
        }

        internal static void Add(this FileIndex index, PackageReference reference, IEnumerable<ResourceMetadata> cindex)
        {
            foreach (var item in cindex)
            {
                index.Add(reference, item);
            }
        }

        internal static void Add(this FileIndex index, PackageReference reference, CanonicalIndex cindex)
        {
            if (cindex.Files is object)
            {
                index.Add(reference, cindex.Files);
            }
        }

        internal static async Task Index(this FileIndex index, IProject project)
        {
            var entries = await project.GetIndex();

            index.Add(PackageReference.None, entries);
        }
    }
}
