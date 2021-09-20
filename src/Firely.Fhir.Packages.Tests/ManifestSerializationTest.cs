using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class ManifestSerializationTest
    {
        [TestMethod]
        public void RoundtripsAllCommonProperties()
        {
            PackageManifest manif = new PackageManifest
            {
                Name = "roundtrip.test",
                Version = "1.0.1",
                Description = "Does this roundtrip all properties?",
                Author = "Ewout",
                Dependencies = new() { ["dep.a"] = "=1.0.0", ["dep.b"] = ">=2.0.1" },
                DevDependencies = new() { ["ddep.a"] = "=1.0.0", ["ddep.b"] = "latest" },
                Keywords = new() { "just", "a", "test" },
                License = "No license",
                Homepage = "http://nu.nl",
                Directories = new() { [PackageManifest.DirectoryKeys.DIRECTORY_KEY_LIB] = "package" },
                Title = "Package with tests",
                FhirVersions = new() { "3.0.2", "4.0.1" },
                FhirVersionList = new() { "3.0.1", "4.0.2" },
                Maintainers = new()
                {
                    new() { Name = "Ewout", Email = "ewout@fire.ly" },
                    new() { Email = "someone@somehwere.net " }
                },
                Canonical = "http://nu.nl/package",
                Url = "http://nu.nl",
                Jurisdiction = "urn:iso:std:iso:3166#US"
            };

            var json = Parser.WriteManifest(manif);
            var manif2 = Parser.ReadManifest(json);

            manif2.Should().BeEquivalentTo(manif);
        }

        [TestMethod]
        public void DoesNotWriteNulls()
        {
            PackageManifest manif = ManifestFile.Create("a-b-c", "4.0.1");
            var json = writeManifest(manif);

            var parsedJson = JObject.Parse(json);

            // we should not find an `'author' : null` property
            Assert.IsFalse(parsedJson.ContainsKey("author"));
        }

        [TestMethod]
        public void MergeDoesNotWriteNulls()
        {
            var dir = CreateTempDir();
            var manifest1 = new PackageManifest { Name = "a-b-c", Version = "4.0.1" };
            var manifest2 = new PackageManifest { Name = "a-b-c", Version = "4.0.1", Author = "Turing" };
            
            var serialized1 = Parser.WriteManifest(manifest1);
            var serialized2 = Parser.JsonMergeManifest(manifest2, serialized1);

            var roundtrip = JObject.Parse(serialized2);

            Assert.IsFalse(roundtrip.ContainsKey("keywords"));
            Assert.IsFalse(roundtrip.ContainsKey("license"));
            Assert.IsFalse(roundtrip.ContainsKey("homepage"));
        }


        private string writeManifest(PackageManifest manif)
        {
            var tempDir = CreateTempDir();
            FolderProject proj = new FolderProject(tempDir);
            proj.WriteManifest(manif);

            return File.ReadAllText(Path.Combine(tempDir, PackageConsts.Manifest));
        }

        private static string CreateTempDir()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            return tempDir;
        }
    }
}
