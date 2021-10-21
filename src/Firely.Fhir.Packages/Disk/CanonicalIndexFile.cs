using System;
using System.IO;

namespace Firely.Fhir.Packages
{
    public static class CanonicalIndexFile
    {
        public const int VERSION = 6;

        public static CanonicalIndex GetFromFolder(string originFolder, string indexDestinationFolder, bool recurse)
        {
            if (ExistsIn(indexDestinationFolder))
            {
                var index = ReadFromFolder(indexDestinationFolder);
                if (index.Version == VERSION) return index;
            }
            // otherwise:
            return Create(originFolder, indexDestinationFolder, recurse);
        }

        public static CanonicalIndex Create(string originFolder, string indexDestinationFolder, bool recurse)
        {
            var entries = CanonicalIndexer.IndexFolder(originFolder, recurse, indexDestinationFolder);
            var index = new CanonicalIndex { Files = entries, Version = VERSION, date = DateTimeOffset.Now };
            WriteToFolder(index, indexDestinationFolder);
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


