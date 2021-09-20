using SemVer;
using System.Collections.Generic;
using System.Linq;

namespace Firely.Fhir.Packages
{
    public class Versions
    {
        readonly List<Version> list = new List<Version>();

        [System.CLSCompliant(false)]
        public IReadOnlyCollection<Version> Items => list;

        public Versions() { }

        public Versions(IEnumerable<string> versions)
        {
            Append(versions);
        }

        public void Append(IEnumerable<string> versions)
        {
            foreach (var s in versions)
            {
                if (TryParseVersion(s, out Version version))
                {
                    list.Add(version);
                }
            }
            list.Sort();
        }

        [System.CLSCompliant(false)]
        public Version Latest(bool previews = false)
        {
            return list.LastOrDefault();
        }

        [System.CLSCompliant(false)]
        public static bool TryParseVersion(string s, out Version v)
        {
            try
            {
                v = new Version(s, loose: true);
                // Loose = true, prevents most errors. 
                // But just to make sure we'll contain any exceptions, since it doesn't have a TryParse. 
                return true;
            }
            catch
            {
                v = null;
                return false;
            }
        }

        [System.CLSCompliant(false)]
        public Version Resolve(Range range)
        {

            return range.MaxSatisfying(list);
        }

        [System.CLSCompliant(false)]
        public bool Has(Version version)
        {
            foreach (var item in list)
            {
                if (item == version) return true;
            }
            return false;
        }

        public bool IsEmpty => list is null || list.Count == 0;
    }
}
