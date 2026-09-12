using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ReplayEventVerifier.Core
{
    /// <summary>
    /// File digests for the manifest, so a capture can be shown to be the same
    /// bytes that were compared.
    /// </summary>
    public static class Hashing
    {
        public static string Sha256File(string path)
        {
            using (var sha = SHA256.Create())
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1 << 16))
            {
                byte[] hash = sha.ComputeHash(fs);
                var sb = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++)
                    sb.Append(hash[i].ToString("x2", CultureInfo.InvariantCulture));
                return sb.ToString();
            }
        }
    }
}
