using System.IO;
using System;

namespace Firely.Fhir.Packages
{
    public static class LockFile
    { 
        public static PackageClosure Read(string path)
        {
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                var dto = Parser.ReadLockFileJson(content);
                return new PackageClosure
                {
                    References = dto.PackageReferences.ToPackageReferences(),
                    Missing = dto.MissingDependencies.ToPackageDependencies()
                };

            }
            else return null;
        }

        public static bool IsOutdated(string folder)
        {
            var man_path = Path.Combine(folder, PackageConsts.Manifest);
            var man_time = File.GetLastWriteTimeUtc(man_path);

            var asset_path = Path.Combine(folder, PackageConsts.LockFile);
            var asset_time = File.GetLastWriteTimeUtc(asset_path);
            return asset_time < man_time;
        }

        public static PackageClosure ReadFromFolder(string folder)
        {
            var path = Path.Combine(folder, PackageConsts.LockFile);
            return Read(path);
        }

        public static PackageClosure ReadFromFolderOrCreate(string folder)
        {
            var path = Path.Combine(folder, PackageConsts.LockFile);
            if (File.Exists(path))
            {
                return Read(path);
            }
            else
            {
                return new PackageClosure();
            }
        }

        public static void WriteToFolder(PackageClosure closure, string folder)
        {
            var dto = CreateLockFileJson(closure);
            var path = Path.Combine(folder, PackageConsts.LockFile);
            Write(dto, path);
        }

        private static void Write(LockFileJson json, string path)
        {
            json.Updated = DateTime.Now;
            var content = Parser.WriteLockFileDto(json);
            File.WriteAllText(path, content);
        }

        private static LockFileJson CreateLockFileJson(PackageClosure closure)
        {
            return new LockFileJson
            {
                PackageReferences = closure.References.ToDictionary(),
                MissingDependencies = closure.Missing.ToDictionary()
            };
        }
    }
}


