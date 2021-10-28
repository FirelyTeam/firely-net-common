using System;
using System.IO;

namespace Firely.Fhir.Packages
{
    public static class CanonicalIndexFile
    {
        public const int VERSION = 6;

        public static CanonicalIndex GetFromFolder(string folder, bool recurse)
        {
            if (ExistsIn(folder))
            {
                var index = ReadFromFolder(folder);
                if (index.Version == VERSION) return index;
            }
            // otherwise:
            return Create(folder, recurse);
        }

        public static CanonicalIndex Create(string folder, bool recurse)
        {
            var entries = CanonicalIndexer.IndexFolder(folder, recurse);
            var index = new CanonicalIndex { Files = entries, Version = VERSION, date = DateTimeOffset.Now };
            WriteToFolder(index, folder);
            return index;
        }

        public static CanonicalIndex ReadFromFolder(string folder)
        {
            var path = Path.Combine(folder, PackageConsts.CanonicalIndexFile);
            return Read(path);
        }

        public static CanonicalIndex Read(string path)
        {
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                return Parser.ReadCanonicalIndex(content);

            }
            else return null;
        }

        private static void Write(CanonicalIndex index, string path)
        {
            var content = Parser.WriteCanonicalIndex(index);
            File.WriteAllText(path, content);
        }

        private static bool ExistsIn(string folder)
        {
            var path = Path.Combine(folder, PackageConsts.CanonicalIndexFile);
            return File.Exists(path);
        }

        private static void WriteToFolder(CanonicalIndex index, string folder)
        {
            var path = Path.Combine(folder, PackageConsts.CanonicalIndexFile);
            Write(index, path);
        }





    }
}


