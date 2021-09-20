using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public static class PackageClientExtensions
    {
        public static async ValueTask<string?> DownloadListingRawAsync(this PackageClient client, PackageReference reference)
        {
            if (reference.Version != null && reference.Version.StartsWith("git"))
            {
                throw new NotImplementedException("We cannot yet resolve git references");
            }

            return await client.DownloadListingRawAsync(reference.Name);
        }


        public static async ValueTask<IList<string>> FindPackageByName(this PackageClient client, string partial)
        {
            // backwards compatibility
            var result = await client.CatalogPackagesAsync(pkgname: partial);
            return result.Select(c => c.Name).ToList();
        }

        public static async ValueTask<IList<string>> FindPackagesByCanonical(this PackageClient client, string canonical)
        {
            // backwards compatibility
            var result = await client.CatalogPackagesAsync(canonical: canonical);
            return result.Select(c => c.Name).ToList();
        }
        
    }
}
