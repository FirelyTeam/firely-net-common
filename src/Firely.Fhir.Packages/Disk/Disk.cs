using System.IO;

namespace Firely.Fhir.Packages
{
    public static class Disk
    {
        public static string GetFolderName(string path)
        {
            return new DirectoryInfo(path).Name;
        }
        
     }
}


