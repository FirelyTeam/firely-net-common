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

        public PackageFileReference ResolveCanonical(string canonical)
        {
            return this.FirstOrDefault(r => r.Canonical == canonical);
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
