using System;
using System.Security.Cryptography;
using System.Text;

namespace InventoryAPI.Helpers
{
    public static class PasswordHelper
    {
        // PBKDF2 implementation
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            byte[] hashBytes = new byte[49]; // 16 salt + 32 hash + 1 version
            hashBytes[0] = 0x01; // version
            Buffer.BlockCopy(salt, 0, hashBytes, 1, 16);
            Buffer.BlockCopy(hash, 0, hashBytes, 17, 32);

            return Convert.ToBase64String(hashBytes);
        }

        public static bool Verify(string password, string hashed)
        {
            try
            {
                byte[] hashBytes = Convert.FromBase64String(hashed);
                if (hashBytes.Length != 49 || hashBytes[0] != 0x01)
                    return false;

                byte[] salt = new byte[16];
                Buffer.BlockCopy(hashBytes, 1, salt, 0, 16);
                byte[] storedHash = new byte[32];
                Buffer.BlockCopy(hashBytes, 17, storedHash, 0, 32);

                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
                byte[] computed = pbkdf2.GetBytes(32);

                return CryptographicOperations.FixedTimeEquals(storedHash, computed);
            }
            catch
            {
                return false;
            }
        }
    }
}
