using Firely.Fhir.Packages;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System.IO;
using System.Threading.Tasks;

namespace Hl7.Fhir.Specification.Source
{

    public class NpmPackageResolver : IAsyncResourceResolver
    {
        private PackageContext _context;
        private ModelInspector _provider;

        public NpmPackageResolver(string filePath, ModelInspector provider)
        {
            _context = createPackageContextAsync(filePath).Result;
            _provider = provider;
        }

        private async Task<PackageContext> createPackageContextAsync(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"File was not found: '{path}'.");

            var scopePath = Path.Combine(Directory.GetCurrentDirectory(), "package-" + Path.GetRandomFileName());
            if (!Directory.Exists(scopePath))
            {
                Directory.CreateDirectory(scopePath);
            }

            var scope = createContext(scopePath);

            var packageManifest = Packaging.ExtractManifestFromPackageFile(path);
            var reference = packageManifest.GetPackageReference();
            await scope.Cache.Install(reference, path);

            var dependency = new PackageDependency(reference.Name, reference.Version);
            if (reference.Found)
            {

                var manifest = await scope.Project.ReadManifest();
                manifest ??= ManifestFile.Create("temp", packageManifest.GetFhirVersion());
                if (manifest.Name == reference.Name)
                {
                    throw new("Skipped updating package manifest because it would cause the package to reference itself."); // should this not be a general rule in the package installer?
                }
                else
                {
                    manifest.AddDependency(dependency);
                    await scope.Project.WriteManifest(manifest);
                    await scope.Restore();
                }
            }

            return scope;
        }

        public async Task<Resource?> ResolveByCanonicalUriAsync(string uri)
        {
            (var url, var version) = splitCanonical(uri);
            var content = await _context.GetFileContentByCanonical(url);
            return toResource(content);
        }

        public async Task<Resource> ResolveByUriAsync(string uri)
        {
            (string resource, string id) = uri.Splice('/');
            var content = await _context.GetFileContentById(resource, id);
            return content is null ? null : toResource(content);
        }

        private static (string url, string? version) splitCanonical(string canonical)
        {
            if (canonical.EndsWith("|"))
                canonical = canonical.Substring(0, canonical.Length - 1);

            var position = canonical.LastIndexOf('|');

            return position == -1 ?
                (canonical, null)
                : (canonical.Substring(0, position), canonical.Substring(position + 1));
        }

        private Resource toResource(string content)
        {
            var sourceNode = FhirJsonNode.Parse(content);
            return TypedSerialization.ToPoco(sourceNode, _provider) as Resource;
        }

        private PackageContext createContext(string folder, bool localCache = false)
        {
            var client = PackageClient.Create();
            string cache_folder = localCache ? folder : null;
            var cache = new DiskPackageCache(cache_folder);
            var project = new FolderProject(folder);
            return new PackageContext(cache, project, client);
        }

    }
}
