using System.Security.Cryptography;
using System.Text;

namespace Firely.Fhir.Packages
{

    public static class CheckSum
    {
        public static byte[] ShaSum(byte[] buffer)
        {
            using var sha = new SHA1Managed();
            var hash = sha.ComputeHash(buffer);
            return hash;
        }

        public static string HashToHexString(byte[] hash)
        {
            var builder = new StringBuilder();
            foreach (var @byte in hash)
            {
                builder.Append(@byte.ToString("x2"));
            }
            return builder.ToString();
        }
    }

}


