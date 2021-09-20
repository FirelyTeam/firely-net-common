using System.Collections.Generic;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public interface IPackageCache : IPackageServer
    {
        Task<bool> IsInstalled(PackageReference reference);
        public Task<IEnumerable<PackageReference>> GetPackageReferences();

        Task Install(PackageReference reference, byte[] buffer);
        Task<PackageManifest> ReadManifest(PackageReference reference);
        Task<CanonicalIndex> GetCanonicalIndex(PackageReference reference);
        Task<string> GetFileContent(PackageReference reference, string filename);
    }
}

