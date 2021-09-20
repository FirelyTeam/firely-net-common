using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class ChecksumTest
    {
        [TestMethod]
        public void TestFhirPackage()
        {
            // This one should fail, because Simplifier has not yet implemented adding checksums.
            TestPackageChecksum(PackageUrlProviders.Simplifier, "hl7.fhir.r4.core@4.0.1");
        }

        [TestMethod]
        public void TestNpmPackage()
        {
            TestPackageChecksum(PackageUrlProviders.Npm, "jquery@3.5.1");
        }

        public void TestPackageChecksum(IPackageUrlProvider provider, PackageReference reference)
        {
            PackageClient client = new(provider);

            var listing = client.DownloadListingAsync(reference.Name).Result;

            var release = listing.Versions[reference.Version];
            var original_hash = release.Dist.Shasum;

            var buffer = client.GetPackage(reference).Result;
            
            var bytehash = CheckSum.ShaSum(buffer);
            var hash = CheckSum.HashToHexString(bytehash);
            Assert.AreEqual(original_hash, hash);
        }
    }
}
