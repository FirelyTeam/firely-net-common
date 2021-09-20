using System.Collections.Generic;

namespace Firely.Fhir.Packages
{
    public static class PackageReferenceExtensions
    {
        
        public static Dictionary<string, string> ToDictionary(this IEnumerable<PackageReference> references)
        {
            var dict = new Dictionary<string, string>();
            foreach(var reference in references)
            {
                dict.Add(reference.Name, reference.Version);
            }
            return dict;
        }

        public static Dictionary<string, string> ToDictionary(this IEnumerable<PackageDependency> references)
        {
            var dict = new Dictionary<string, string>();
            foreach (var reference in references)
            {
                dict.Add(reference.Name, reference.Range);
            }
            return dict;
        }

        public static List<PackageReference> ToPackageReferences(this Dictionary<string, string> dict)
        {
            var list = new List<PackageReference>();
            foreach(var item in dict)
            {
                list.Add(item); // implicit converion
            }
            return list;
        }

        public static List<PackageDependency> ToPackageDependencies(this Dictionary<string, string> dict)
        {
            var list = new List<PackageDependency>();
            foreach (var item in dict)
            {
                list.Add(item); // implicit converion
            }
            return list;
        }

        public static IEnumerable<PackageDependency> GetDependencies(this PackageManifest manifest)
        {
            if (manifest.Dependencies is null) yield break;

            foreach (PackageDependency dep in manifest.Dependencies)
            {
                yield return dep;
            }
        }

        public static string GetNpmName(this PackageReference reference)
        {
            return (reference.Scope == null) ? reference.Name : $"@{reference.Scope}%2F{reference.Name}";
        }

    }
}
