using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public class FolderProject : IProject
    {
        public string Folder { get; private set; }

        public FolderProject(string folder)
        {
            this.Folder = folder;
        }

        public Task<List<ResourceMetadata>> GetIndex()
        {
            // this should be cached, but we need to bust it on changes.
            return Task.FromResult(CanonicalIndexer.IndexFolder(Folder, recurse: true));
        }

        /// <summary>
        /// Reads the raw contents of the given file.
        /// </summary>
        /// <param name="filename">The name of a file within the given <see cref="Folder"/>.</param>
        /// <returns></returns>
        public Task<string> GetFileContent(string filename)
        {
            var path = Path.Combine(Folder, filename);
            return Task.FromResult(File.ReadAllText(path));
        }

        /// <summary>
        /// Reads and parses a <see cref="PackageClosure"/> from the <see cref="Folder"/>.
        /// </summary>
        /// <returns></returns>
        public Task<PackageClosure> ReadClosure()
        {
            var closure = LockFile.ReadFromFolder(Folder);
            return Task.FromResult(closure);
        }

        /// <summary>
        /// Reads and parses a <see cref="PackageManifest"/> from the <see cref="Folder"/>.
        /// </summary>
        /// <returns></returns>
        public Task<PackageManifest> ReadManifest()
        {
            var manifest = ManifestFile.ReadFromFolder(Folder);
            return Task.FromResult(manifest); ;
        }

        public Task WriteClosure(PackageClosure closure)
        {
            LockFile.WriteToFolder(closure, Folder);

            return Task.FromResult(0); //because in net45 there is no Task.CompletedTask (Paul)
        }

        public Task WriteManifest(PackageManifest manifest)
        {
            ManifestFile.WriteToFolder(manifest, Folder, merge: true);

            return Task.FromResult(0); //because in net45 there is no Task.CompletedTask (Paul)
        }
    }

}
