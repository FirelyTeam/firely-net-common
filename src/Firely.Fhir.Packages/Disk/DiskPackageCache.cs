using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public class DiskPackageCache : IPackageCache
    {
        public readonly string Root;

        public DiskPackageCache(string? root = null)
        {
            this.Root = root ?? Platform.GetFhirPackageRoot();
        }

        public Task<bool> IsInstalled(PackageReference reference)
        {
            string target = PackageContentFolder(reference);
            return Task.FromResult(Directory.Exists(target));
        }

        public async Task Install(PackageReference reference, byte[] buffer)
        {
            var folder = PackageRootFolder(reference);
            await Packaging.UnpackToFolder(buffer, folder);
            CreateIndexFile(reference);
        }

        public Task<PackageManifest?> ReadManifest(PackageReference reference)
        {
            var folder = PackageContentFolder(reference);

            return Directory.Exists(folder)
                ? Task.FromResult(ManifestFile.ReadFromFolder(folder))
                : null;
        }

        public Task<CanonicalIndex> GetCanonicalIndex(PackageReference reference)
        {
            var rootFolder = PackageRootFolder(reference);
            return Task.FromResult(CanonicalIndexFile.GetFromFolder(rootFolder, recurse: true));
        }

        public string PackageContentFolder(PackageReference reference)
        {
            //// for backwards compatibility:
            //{
            //    var pkgfolder = PackageFolderName(reference, '-');
            //    var folder = Path.Combine(Root, pkgfolder, PackageConsts.PackageFolder);
            //    if (Directory.Exists(folder)) return folder;
            //}

            // the new way:
            {
                var pkgfolder = PackageFolderName(reference, '#');
                var folder = Path.Combine(Root, pkgfolder, PackageConsts.PackageFolder);
                return folder;
            }
        }

        private string PackageRootFolder(PackageReference reference)
        {
            var pkgfolder = PackageFolderName(reference);
            string target = Path.Combine(Root, pkgfolder);
            return target;
        }

        public Task<IEnumerable<PackageReference>> GetPackageReferences()
        {
            var folders = GetPackageRootFolders();
            var references = new List<PackageReference>(folders.Count());

            foreach (var folder in folders)
            {
                var entry = Disk.GetFolderName(folder);
                var idx = entry.IndexOfAny(new[] { '-', '#' }); // backwards compatibility: also support '-'

                references.Add(new PackageReference
                {
                    Name = entry.Substring(0, idx),
                    Version = entry.Substring(idx + 1)
                });
            }

            return Task.FromResult(references.AsEnumerable());
        }

        public Task<string> GetFileContent(PackageReference reference, string filename)
        {

            var folder = PackageRootFolder(reference);
            string path = Path.Combine(folder, filename);

            string content;
            try
            {
                content = File.ReadAllText(path);
                return Task.FromResult(content);
            }
            catch
            {
                throw new Exception($"The file {filename} could not be found in package {reference}. You might have to do a restore.");
            }
        }

        public async Task<Versions> GetVersions(string name)
        {
            var references = await GetPackageReferences();
            var vlist = references.Where(r => r.Name == name).Select(r => r.Version);
            var versions = new Versions(vlist);

            return versions;
        }

        [Obsolete("Not implemented yet")]
        public Task<byte[]> GetPackage(PackageReference reference)
        {
            throw new NotImplementedException();
        }

        private void CreateIndexFile(PackageReference reference)
        {
            var rootFolder = PackageRootFolder(reference);
            CanonicalIndexFile.Create(rootFolder, recurse: true);
        }

        private static string PackageFolderName(PackageReference reference, char glue = '#')
        {
            return reference.Name + glue + reference.Version;
        }

        private IEnumerable<string> GetPackageRootFolders()
        {
            if (Directory.Exists(Root))
            {
                return Directory.GetDirectories(Root);
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }


    }


}

