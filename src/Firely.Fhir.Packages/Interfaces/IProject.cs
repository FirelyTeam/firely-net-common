using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// Only used to get access to the project I/O, this is not about scope
    /// </summary>
    public interface IProject 
    {
        Task<PackageManifest> ReadManifest();
        Task WriteManifest(PackageManifest manifest);
        Task<PackageClosure> ReadClosure();
        Task WriteClosure(PackageClosure closure);

        public Task<string> GetFileContent(string filename);
        public Task<List<ResourceMetadata>> GetIndex();
    }
}
