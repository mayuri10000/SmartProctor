using System.Security.Cryptography;
using System.Text;

namespace SmartProctor.Server.Utils
{
    /// <summary>
    /// The helper for hashing passwords with salt.
    /// </summary>
    public static class MD5Helper
    {
        /// <summary>
        /// Helper function for hashing the password with MD5.
        /// User name and additional string will be used as salt.
        /// </summary>
        /// <param name="userName">User name (used as salt)</param>
        /// <param name="password">Password to be hashed</param>
        /// <returns>Hashed password</returns>
        public static string HashPassword(string userName, string password)
        {
            var clearText = "*Mayuri*" + userName + "&Kotori&" + password;
            return HashString(clearText);
        }
        
        private static string HashString(string str)
        {
            var md5 = MD5.Create();
            var byteOld = Encoding.UTF8.GetBytes(str);
            var byteNew =  md5.ComputeHash(byteOld);
            var sb = new StringBuilder();
            // Convert result to HEX string
            foreach (var b in byteNew)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}