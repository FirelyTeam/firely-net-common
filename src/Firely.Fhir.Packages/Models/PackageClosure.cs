using SemVer;
using System.Collections.Generic;

namespace Firely.Fhir.Packages
{
    public class PackageClosure
    {
        public bool Complete => Missing.Count == 0;

        public List<PackageReference> References = new List<PackageReference>();
        public List<PackageDependency> Missing = new List<PackageDependency>();

        public bool Add(PackageReference reference)
        {
            if (Find(reference.Name, out var existing))
            {
                if (existing == reference) return false;
                
                var highest = Highest(reference, existing);
                if (highest != existing)
                {
                    References.Remove(existing);
                    References.Add(highest);
                    return true;
                }
                else
                {
                    return false;
                }
                
            }
            else 
            {
                References.Add(reference);
                return true;
            }
        }

        public PackageReference Highest(PackageReference A, PackageReference B)
        {
            var versionA = new Version(A.Version);
            var versionB = new Version(B.Version);
            var highest = (versionA > versionB) ? A : B;

            return highest;
        }

        public bool Find(string pkgname, out PackageReference reference)
        {
            foreach(var refx in References)
            {
                if (string.Compare(refx.Name, pkgname, ignoreCase: true) == 0)
                {
                    reference = refx;
                    return true;
                }
            }
            reference = default;
            return false; 
        }

        public void AddMissing(PackageDependency reference)
        {
            if (!Missing.Contains(reference)) Missing.Add(reference);
        }

    }

}
