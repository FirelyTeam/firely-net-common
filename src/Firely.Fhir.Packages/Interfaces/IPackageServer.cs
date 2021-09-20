using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public interface IPackageServer
    {
        Task<Versions> GetVersions(string name);
        Task<byte[]> GetPackage(PackageReference reference);
    }
}
