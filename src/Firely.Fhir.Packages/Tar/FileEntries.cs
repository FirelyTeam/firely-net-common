using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Firely.Fhir.Packages
{
    public static class FileEntries
    {
        public static IEnumerable<FileEntry> ReadFileEntries(string folder, string pattern)
        {
            foreach (var filepath in Directory.GetFiles(folder, pattern))
            {
                yield return ReadFileEntry(filepath);
            }
        }

        
        public static bool Match(this FileEntry file, string filename)
        {
            return string.Compare(file.FileName, filename, ignoreCase: true) == 0;
        }

        public static bool HasExtension(this FileEntry file, params string[] extensions)
        {
            var extension = Path.GetExtension(file.FileName);
            foreach(var ext in extensions)
            {
                if (string.Compare(extension, ext, ignoreCase: true) == 0) return true;
            }
            return false;
        }

        public static IEnumerable<string> AllFilesToPack(string folder)
        {
            return Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
        }

        public static IEnumerable<FileEntry> ReadAllFilesToPack(string folder)
        {
            return AllFilesToPack(folder).Select(ReadFileEntry);
        }

        public static FileEntry ReadFileEntry(string filepath)
        {
            var buffer = File.ReadAllBytes(filepath);
            var entry = new FileEntry { FilePath = filepath, Buffer = buffer };
            return entry;
        }

        public static IEnumerable<FileEntry> ChangeFolder(this IEnumerable<FileEntry> entries, string folder)
        {
            foreach (var entry in entries)
            {
                yield return entry.ChangeFolder(folder);    
            }
        }

        public static FileEntry ChangeFolder(this FileEntry entry, string folder)
        {
            string filename = Path.GetFileName(entry.FilePath);
            entry.FilePath = Path.Combine(folder, Path.GetFileName(filename));
            return entry;
        }

        public static IEnumerable<FileEntry> MakePathsRelative(this IEnumerable<FileEntry> files, string root)
        {
            foreach (var file in files)
                yield return file.MakeRelativePath(root);
        }


        private static Uri MakeFolderUri(string folder)
        {
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            return new Uri(folder);
        }

        public static string MakeRelativePath(string path, string root)
        {
            Uri pathUri = new Uri(path);
            Uri rootUri = MakeFolderUri(root);
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        public static FileEntry MakeRelativePath(this FileEntry file, string root)
        {
            file.FilePath = MakeRelativePath(file.FilePath, root);
            return file;
        }

        static string FOLDER_OTHER = Path.Combine(PackageConsts.PackageFolder, "other");

        /// <summary>
        /// This is a basic implementation to move the package manifest and all resources to the package folder and 
        /// all other files to packages/other. You can write and inject your own implementation when packaging a folder
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static FileEntry OrganizeToPackageStructure(this FileEntry file)
        {
            if (file.Match(PackageConsts.Manifest))
                return file.ChangeFolder(PackageConsts.PackageFolder);

            else if (file.HasExtension(".xml", ".json"))
                return file.ChangeFolder(PackageConsts.PackageFolder);

            else
                return file.ChangeFolder(FOLDER_OTHER);
        }

    }

}


