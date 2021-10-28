
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Firely.Fhir.Packages
{

    public class PackageListing
    {
        [JsonProperty(PropertyName = "_id")]
        public string? Id;

        [JsonProperty(PropertyName = "name")]
        public string? Name;

        [JsonProperty(PropertyName = "description")]
        public string? Description;

        [JsonProperty(PropertyName = "dist-tags")]
        public Dictionary<string, string>? DistTags;

        [JsonProperty(PropertyName = "versions")]
        public Dictionary<string, PackageRelease>? Versions;
    }

    public class PackageRelease
    {
        [JsonProperty(PropertyName = "name")]
        public string? Name;

        [JsonProperty(PropertyName = "version")]
        public string? Version;

        [JsonProperty(PropertyName = "description")]
        public string? Description;

        [JsonProperty(PropertyName = "dist")]
        public Dist? Dist;

        // Removed the property, because in NPM-6 it's a string and in NPM-7 it's a subclass.
        // The horror!
        //[JsonProperty(PropertyName = "author")]
        //public string? Author;

        [JsonProperty(PropertyName = "fhirVersion")]
        public string? FhirVersion;

        [JsonProperty(PropertyName = "url")]
        public string? Url;

        [JsonProperty(PropertyName = "unlisted")]
        public string? Unlisted;
    }

    public class Dist
    {
        [JsonProperty(PropertyName = "shasum")]
        public string Shasum;

        [JsonProperty(PropertyName = "tarball")]
        public string Tarball;
    }

    public class PackageManifest
    {
        /// <summary>
        /// The globally unique name for the package.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name;

        /// <summary>
        /// Semver-based version for the package
        /// </summary>
        [JsonProperty(PropertyName = "version")]
        public string Version;

        /// <summary>
        /// Description of the package.
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string? Description;

        /// <summary>
        /// Author of the package.
        /// </summary>
        [JsonProperty(PropertyName = "author")]
        public string? Author;

        /// <summary>
        /// Other packages that the contents of this packages depend on.
        /// </summary>
        [JsonProperty(PropertyName = "dependencies")]
        public Dictionary<string, string>? Dependencies;

        /// <summary>
        /// Other packages necessary during development of this package.
        /// </summary>
        [JsonProperty(PropertyName = "devDependencies")]
        public Dictionary<string, string>? DevDependencies;

        /// <summary>
        /// List of keywords to help with discovery.
        /// </summary>
        [JsonProperty(PropertyName = "keywords")]
        public List<string>? Keywords;

        /// <summary>
        /// List of keywords to help with discovery.
        /// </summary>
        [JsonProperty(PropertyName = "license")]
        public string? License;

        /// <summary>
        /// The url to the project homepage.
        /// </summary>
        [JsonProperty(PropertyName = "homepage")]
        public string? Homepage;

        /// <summary>
        /// Describes the structure of the package.
        /// </summary>
        /// <remarks>Some of the common keys used are defined in <see cref="DirectoryKeys"/>.</remarks>
        [JsonProperty(PropertyName = "directories")]
        public Dictionary<string, string> Directories;

        /// <summary>
        /// String-based keys used in the <see cref="Directories"/> dictionary.
        /// </summary>
        public class DirectoryKeys
        {
            /// <summary>
            /// Where the bulk of the library is.
            /// </summary>
            public const string DIRECTORY_KEY_LIB = "lib";
            public const string DIRECTORY_KEY_BIN = "bin";
            public const string DIRECTORY_KEY_MAN = "man";
            public const string DIRECTORY_KEY_DOC = "doc";
            public const string DIRECTORY_KEY_EXAMPLE = "example";
            public const string DIRECTORY_KEY_TEST = "test";
        }

        /// <summary>
        /// Title for the package.
        /// </summary>
        [JsonProperty(PropertyName = "title")]
        public string? Title;

        /// <summary>
        /// Versions of the FHIR standard used in artifacts within this package.
        /// </summary>
        /// <remarks>Largely obsolete, and replaced by actual dependencies on the
        /// core packages.</remarks>
        [JsonProperty(PropertyName = "fhirVersions")]
        public List<string>? FhirVersions;

        /// <summary>
        /// Versions of the FHIR standard used in artifacts within this package.
        /// </summary>
        /// <remarks>It seems this is mistakenly generated in the core packages
        /// published by HL7 and should be the same as <see cref="FhirVersions"/> above.</remarks>
        [JsonProperty(PropertyName = "fhir-version-list")]
        public List<string>? FhirVersionList;

        public class Maintainer
        {
            [JsonProperty(PropertyName = "name")]
            public string? Name;

            [JsonProperty(PropertyName = "email")]
            public string? Email;
        }

        /// <summary>
        /// List of individual(s) responsible for maintaining the package.
        /// </summary>
        [JsonProperty(PropertyName = "maintainers")]
        public List<Maintainer>? Maintainers;

        /// <summary>
        /// For IG packages: The canonical url of the IG (equivalent to ImplementationGuide.url).
        /// </summary>
        [JsonProperty(PropertyName = "canonical")]
        public string? Canonical;

        /// <summary>
        /// For IG packages: Where the human readable representation (e.g. IG) is published on the web.
        /// </summary>
        [JsonProperty(PropertyName = "url")]
        public string? Url;

        /// <summary>
        /// Country code for the jurisdiction under which this package is published.
        /// </summary>
        /// <remarks>Formatted as an urn specifying the code system and code, e.g. "urn:iso:std:iso:3166#US".</remarks>
        [JsonProperty(PropertyName = "jurisdiction")]
        public string? Jurisdiction;
    }

    public class LockFileJson
    {
        [JsonProperty(PropertyName = "updated")]
        public DateTime Updated;

        [JsonProperty(PropertyName = "dependencies")]
        public Dictionary<string, string> PackageReferences;

        [JsonProperty(PropertyName = "missing")]
        public Dictionary<string, string> MissingDependencies;
    }


    public class CanonicalIndex
    {
        public DateTimeOffset date;

        [JsonProperty(PropertyName = "index-version")]
        public int Version;

        [JsonProperty(PropertyName = "files")]
        public List<ResourceMetadata>? Files; // canonical -> file
    }

    public class ResourceMetadata
    {
        [JsonProperty("filename")]
        public string FileName;

        [JsonProperty("filepath")]
        public string FilePath;

        [JsonProperty("resourceType")]
        public string ResourceType;

        [JsonProperty("id")]
        public string Id;

        [JsonProperty("url")]
        public string? Canonical;

        [JsonProperty("version")]
        public string? Version;

        [JsonProperty("kind")]
        public string? Kind;

        [JsonProperty("type")]
        public string? Type;

        [JsonProperty("fhirVersion")]
        public string? FhirVersion;

        //Firely specific attribute used to make choices when multiple artifacts with the same canonical URL and version are found.
        [JsonProperty("firely-hasSnapshot")]
        public bool HasSnapshot;

        //Firely specific attribute used to make choices when multiple artifacts with the same canonical URL and version are found.
        [JsonProperty("firely-hasExpansion")]
        public bool HasExpansion;

        public void CopyTo(ResourceMetadata other)
        {
            other.FileName = FileName;
            other.FilePath = FilePath;
            other.ResourceType = ResourceType;
            other.Id = Id;
            other.Canonical = Canonical;
            other.Version = Version;
            other.Kind = Kind;
            other.Type = Type;
            other.FhirVersion = FhirVersion;
            other.HasExpansion = HasExpansion;
            other.HasSnapshot = HasSnapshot;
        }
    }

    public class PackageCatalogEntry
    {
        public string Name;
        public string Description;
        public string FhirVersion;
    }

    public static class JsonModelExtensions
    {
        public static PackageReference GetPackageReference(this PackageManifest manifest)
        {
            var reference = new PackageReference(manifest.Name, manifest.Version);
            return reference;
        }

        public static IEnumerable<PackageReference> GetPackageReferences(this LockFileJson dto)
        {
            foreach (var item in dto.PackageReferences) yield return item; // implicit conversion
        }

        public static void AddDependency(this PackageManifest manifest, string name, string version)
        {
            if (version is null) version = "latest";
            if (manifest.Dependencies is null) manifest.Dependencies = new Dictionary<string, string>();
            if (!manifest.Dependencies.ContainsKey(name))
            {
                manifest.Dependencies.Add(name, version);
            }
            else
            {
                manifest.Dependencies[name] = version;
            }
        }

        public static void AddDependency(this PackageManifest manifest, PackageDependency dependency)
        {
            manifest.AddDependency(dependency.Name, dependency.Range);
        }

        public static void AddDependency(this PackageManifest manifest, PackageManifest dependency)
        {
            manifest.AddDependency(dependency.Name, dependency.Version);
        }

        public static bool HasDependency(this PackageManifest manifest, string pkgname)
        {
            foreach (var key in manifest.Dependencies.Keys)
            {
                if (string.Compare(key, pkgname, ignoreCase: true) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool RemoveDependency(this PackageManifest manifest, string pkgname)
        {
            foreach (var key in manifest.Dependencies.Keys)
            {
                if (string.Compare(key, pkgname, ignoreCase: true) == 0)
                {
                    manifest.Dependencies.Remove(key);
                    return true;
                }
            }
            return false;
        }

        public static string GetFhirVersion(this PackageManifest manifest)
        {
            string version =
                manifest.FhirVersions?.FirstOrDefault()
                ?? manifest.FhirVersionList?.FirstOrDefault();

            return version;
        }

        public static void SetFhirVersion(this PackageManifest manifest, string version)
        {
            manifest.FhirVersions = new List<string> { version };
        }
    }
}
