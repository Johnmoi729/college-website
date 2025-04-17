using System.Security.Cryptography;
using System.Text;

namespace CollegeWebsite.Utilities
{
    public static class PasswordHasher
    {
        // Simple hash function (in a real app, use a more robust solution like BCrypt)
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static bool VerifyPassword(string password, string hash)
        {
            string computedHash = HashPassword(password);
            return computedHash == hash;
        }
    }
}