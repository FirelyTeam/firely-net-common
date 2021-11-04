using System.Collections.Generic;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// A packageDependency defines a version range for a specific package. If you want to target a very specific package version, use PackageReference
    /// A PackageDependency is used in a Manifest, while the PackageReference is used in a Scope or Closure.
    /// </summary>
    public struct PackageDependency
    {
        public string Name;
        public string Range;  // 3.x, 3.1 - 3.3, 1.1 | 1.2

        public PackageDependency(string name, string? range = null)
        {
            this.Name = name;
            this.Range = range;
        }

        public static implicit operator PackageDependency(KeyValuePair<string, string> pair)
        {
            return new PackageDependency(pair.Key, pair.Value);
        }

        public static implicit operator PackageDependency(string reference)
        {
            return new PackageDependency(reference, null); // latest
        }

        public override string ToString()
        {
            string range = string.IsNullOrEmpty(Range) ? "(latest)" : Range;
            return $"{Name} {range}";
        }
    }
}
