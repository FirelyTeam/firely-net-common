using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Firely.Fhir.Packages
{
    public static class ManifestFile
    {
        /// <summary>
        /// Reads and parses a <see cref="PackageManifest"/> at the given path.
        /// </summary>
        /// <param name="path">The full path to the file containing the manifest.</param>
        /// <returns></returns>
        public static PackageManifest Read(string path)
        {
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                return Parser.ReadManifest(content);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reads and parses a <see cref="PackageManifest" /> from the package.json file in the given folder. 
        /// If missing, creates a new package.json file. />
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="fhirVersion"></param>
        /// <returns></returns>
        public static PackageManifest ReadOrCreate(string folder, string fhirVersion)
        {
            var manifest = ReadFromFolder(folder);
            if (manifest is null)
            {
                var name = CleanPackageName(Disk.GetFolderName(folder));
                manifest = Create(name, fhirVersion);
            }
            return manifest;
        }


        /// <summary>
        /// Serializes the manifest to json and writes it to the given path, optionally merging the changes with the contents already at that path. 
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="path">The full path to the file to write the manifest to.</param>
        /// <param name="merge">Whether to first merge the contents of the file at the given path before writing it.</param>
        /// <returns></returns>
        public static void Write(PackageManifest manifest, string path, bool merge = false)
        {
            if (File.Exists(path) && merge)
            {
                var content = File.ReadAllText(path);
                var result = Parser.JsonMergeManifest(manifest, content);
                File.WriteAllText(path, result);
            }
            else
            {
                var content = Parser.WriteManifest(manifest);
                File.WriteAllText(path, content);
            }
        }


        /// <summary>
        /// Reads and parses a <see cref="PackageManifest"/> from a package.json file at a given folder.
        /// </summary>
        /// <param name="folder">The folder containing the package.json file.</param>
        /// <returns></returns>
        public static PackageManifest ReadFromFolder(string folder)
        {
            var path = Path.Combine(folder, PackageConsts.Manifest);
            var manifest = Read(path);
            return manifest;
        }

        /// <summary>
        /// Creates a new <see cref="PackageManifest"/> initialized with sensible default values.
        /// </summary>
        /// <param name="name">A name for the package</param>
        /// <param name="fhirVersion">The FHIR version of the package contents.</param>
        /// <returns></returns>
        public static PackageManifest Create(string name, string fhirVersion)
        {
            var release = FhirVersions.Parse(fhirVersion);
            var version = FhirVersions.FhirVersionFromRelease(release);

            var manifest = new PackageManifest
            {
                Name = name,
                Description = "Put a description here",
                Version = "0.1.0",
                Dependencies = new Dictionary<string, string>()
            };
            manifest.SetFhirVersion(version);
            return manifest;
        }

        ///// <summary>
        ///// Creates a new <see cref="PackageManifest"/> initialized with sensible default values.
        ///// </summary>
        ///// <param name="name">A name for the package</param>
        ///// <param name="fhirReleases">The FHIR version(s) of the package contents.</param>
        ///// <returns></returns>
        //public static PackageManifest Create(string name, FhirRelease[] fhirReleases)
        //{
        //    return new PackageManifest
        //    {
        //        Name = name,
        //        Description = "Put a description here",
        //        Version = "0.1.0",
        //        FhirVersions = new List<string>(fhirReleases.Select(r => FhirVersions.FhirVersionFromRelease(r))),
        //        Dependencies = new Dictionary<string, string>()
        //    };
        //}

        /// <summary>
        /// Creates a new <see cref="PackageManifest"/> initialized with sensible default values.
        /// </summary>
        /// <param name="name">A name for the package</param>
        /// <param name="fhirRelease">The FHIR version of the package contents.</param>
        /// <returns></returns>
        [Obsolete("With the introduction of release 4b, integer-numbered releases are no longer useable.")]
        public static PackageManifest Create(string name, int fhirRelease)
        {
            var fhirVersion = FhirVersions.GetFhirVersion(fhirRelease);
            return Create(name, fhirVersion);
        }

        /// <summary>
        /// Serializes the manifest to json and writes it to the package.json file in the given folder, 
        /// optionally merging the changes with the contents already in the package.json file. 
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="folder">The full path to the directory that contains the package.json file.</param>
        /// <param name="merge">Whether to first merge the contents of the file at the given path before writing it.</param>
        /// <returns></returns>
        public static void WriteToFolder(PackageManifest manifest, string folder, bool merge = false)
        {
            string path = Path.Combine(folder, PackageConsts.Manifest);
            Write(manifest, path, merge);
        }

        public static bool ValidPackageName(string name)
        {
            char[] invalidchars = new char[] { '/', '\\' };
            int i = name.IndexOfAny(invalidchars);
            bool valid = i == -1;
            return valid;
        }

        /// <summary>
        /// Generates an acceptable package name from an chosen name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <remarks>Package names can only contain [A-Za-z], so this function will strip out any characters
        /// not within that range.</remarks>
        public static string CleanPackageName(string name)
        {
            var builder = new StringBuilder();
            foreach (char c in name)
            {
                if (c >= 'A' && c <= 'z') builder.Append(c);
            }
            return builder.ToString();
        }

    }
}


