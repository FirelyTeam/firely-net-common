using System.Collections.Generic;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// A package reference is a reference to a very specific version of a package. When you want to define a a range of versions that may quality, like 3.x, use PackageDependency
    /// A PackageReference is used in a Scope or Closure while a PackageDependency is used in a Manifest.
    /// </summary>
    public struct PackageReference
    {
        public string? Scope;
        public string? Name; // null means empty reference
        public string? Version;

        /// <summary>
        /// Provide the name and optionally the version of the package. 
        /// </summary>
        /// <param name="name">The package name may include the (exact) version if separated with an at @ sign.</param>
        /// <param name="version">Optionally the exact version of the package</param>
        public PackageReference(string name, string version) : this(null, name, version)
        { }
        
        public PackageReference (string scope, string name, string version)
        {
            this.Scope = scope;
            this.Name = name;
            this.Version = version;
        }

        public string Moniker => $"{Name}@{Version}";
        public override string ToString()
        {
            string s = $"{Name}@{Version}";
            if (!Found) s += " (NOT FOUND)";
            return s;
        }

        public static PackageReference None => new PackageReference { Name = null, Version = null };

        public bool NotFound => !Found;

        public bool Found => !(Name is null || Version is null);

        public static implicit operator PackageReference (KeyValuePair<string, string> kvp)
        {
            return new PackageReference(kvp.Key, kvp.Value);
        }

        public static implicit operator PackageReference(string reference)
        {
            return Parse(reference);
        }

        public static bool operator == (PackageReference A, PackageReference B)
        {
            return (A.Name == B.Name && A.Version == B.Version);
        }

        public static bool operator != (PackageReference A, PackageReference B)
        {
            return !(A == B);
        }

        public void Deconstruct(out string name, out string version)
        {
            name = Name;
            version = Version;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PackageReference))
            {
                return false;
            }

            var reference = (PackageReference)obj;
            return this.Name == reference.Name &&
                   this.Version == reference.Version;
        }

        public override int GetHashCode()
        {
            return (Name, Version).GetHashCode();
        }

        public static PackageReference Parse(string reference)
        {
            var (scope, name, version) = ParseReference(reference);
            return new PackageReference(scope, name, version);
        }

        private static (string scope, string name, string version) ParseReference(string reference)
        {
            string scope = null;
            string version = null;

            if (reference.StartsWith("@")) // scope: @scope/name@version
            {
                var segments = reference.Split('/');
                scope = segments[0].Substring(1);
                reference = segments[1];
            }

            var parts = reference.Split("@"); // name@version
            string name = parts[0];
            if (parts.Length > 1) version = parts[1];
            return (scope, name, version);

        }

      
    }
}
