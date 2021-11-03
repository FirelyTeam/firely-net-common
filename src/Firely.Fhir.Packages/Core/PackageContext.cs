using System;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{

    public class PackageContext
    {
        public readonly IPackageCache Cache;
        public readonly IProject Project;
        public readonly IPackageServer Server;
        internal PackageClosure Closure;
        internal readonly Action<string> Report;

        public FileIndex Index => _index ??= BuildIndex().Result; // You cannot have async getters in C#, maybe not make this a property ?! (Paul)
        private FileIndex? _index;

        public PackageContext(IPackageCache cache, IProject project, IPackageServer server, Action<string>? report = null)
        {
            this.Cache = cache;
            this.Project = project;
            this.Server = server;
            this.Report = report;
        }

        private async Task<PackageClosure> ReadClosure()
        {
            Closure = await Project.ReadClosure();
            if (Closure is null) throw new ArgumentException("The folder does not contain a package lock file.");
            return Closure;
        }

        public async Task<FileIndex> BuildIndex()
        {
            this.Closure = await ReadClosure();

            var index = new FileIndex();
            await index.Index(Project);
            await index.Index(Cache, Closure);

            return index;
        }


    }
}
