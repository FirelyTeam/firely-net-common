using System.IO;

namespace Firely.Fhir.Packages
{
    public class FileEntry
    {
        public string FilePath;
        public byte[] Buffer;

        public Stream GetStream()
        {
            return new MemoryStream(Buffer);
        }

        public string FileName => Path.GetFileName(FilePath);

    }

}


