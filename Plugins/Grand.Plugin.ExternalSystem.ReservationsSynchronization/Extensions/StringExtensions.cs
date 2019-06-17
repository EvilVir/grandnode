using System.Security.Cryptography;
using System.Text;

namespace Grand.Plugin.ExternalSystem.ReservationsSynchronization.Extensions
{
    public static class StringExtensions
    {
        public static string HashMD5(this string input)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }

                return sb.ToString();
            }
        }
    }
}
