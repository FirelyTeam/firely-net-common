using Firely.Fhir.Packages;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Hl7.Fhir.Specification.Source
{

    public class NpmPackageResolver : IAsyncResourceResolver, IArtifactSource
    {
        private PackageContext _context;
        private ModelInspector _provider;

        public NpmPackageResolver(ModelInspector provider, params string[] filePaths)
        {
            _context = createPackageContextAsync(filePaths).Result;
            _provider = provider;
        }

        private static async Task<PackageContext> createPackageContextAsync(params string[] paths)
        {
            foreach (var path in paths)
            {
                if (!File.Exists(path))
                    throw new FileNotFoundException($"File was not found: '{path}'.");
            }

            var scopePath = Path.Combine(Directory.GetCurrentDirectory(), "package-" + Path.GetRandomFileName());
            if (!Directory.Exists(scopePath))
            {
                Directory.CreateDirectory(scopePath);
            }

            var scope = createContext(scopePath);

            foreach (var path in paths)
            {
                await intstallPackageFromPath(scope, path);
            }

            return scope;
        }

        private static async Task intstallPackageFromPath(PackageContext scope, string path)
        {
            var packageManifest = Packaging.ExtractManifestFromPackageFile(path);
            var reference = packageManifest.GetPackageReference();
            await scope.Cache.Install(reference, path);

            var dependency = new PackageDependency(reference.Name ?? "", reference.Version);
            if (reference.Found)
            {
                var manifest = await scope.Project.ReadManifest();
                manifest ??= ManifestFile.Create("temp", packageManifest.GetFhirVersion());
                if (manifest.Name == reference.Name)
                {
                    throw new("Skipped updating package manifest because it would cause the package to reference itself.");
                }
                else
                {
                    manifest.AddDependency(dependency);
                    await scope.Project.WriteManifest(manifest);
                    await scope.Restore();
                }
            }
        }

        public async Task<Resource?> ResolveByCanonicalUriAsync(string uri)
        {
            (var url, var version) = splitCanonical(uri);
            var content = await _context.GetFileContentByCanonical(url, version);
            return content is null ? null : toResource(content);
        }

        public async Task<Resource?> ResolveByUriAsync(string uri)
        {
            (string resource, string id) = uri.Splice('/');
            var content = await _context.GetFileContentById(resource, id);
            return content is null ? null : toResource(content);
        }

        private static (string url, string version) splitCanonical(string canonical)
        {
            if (canonical.EndsWith("|"))
                canonical = canonical.Substring(0, canonical.Length - 1);

            var position = canonical.LastIndexOf('|');

            return position == -1 ?
                (canonical, "")
                : (canonical.Substring(0, position), canonical.Substring(position + 1));
        }

        private Resource? toResource(string content)
        {
            var sourceNode = FhirJsonNode.Parse(content);
            return TypedSerialization.ToPoco(sourceNode, _provider) as Resource;
        }

        private static PackageContext createContext(string folder, bool localCache = false)
        {
            string? cache_folder = localCache ? folder : null;
            var cache = new DiskPackageCache(cache_folder);
            var project = new FolderProject(folder);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            return new PackageContext(cache, project, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        public IEnumerable<string> ListArtifactNames()
        {
            return _context.GetFileNames();
        }

        public Stream? LoadArtifactByName(string artifactName)
        {
            var content = _context.GetFileContentByFileName(artifactName).Result;
            return content == null ? null : new MemoryStream(Encoding.UTF8.GetBytes(content));
        }
    }
}

#nullable restore
