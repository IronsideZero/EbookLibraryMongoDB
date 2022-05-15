using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace EbookLibraryMongoDB.Services
{
    public static class PasswordService
    {
        /*Method to salt and hash a plaintext password. Returns the hashed password, and outputs the salt as a byte array. 
         * 
         * Taken in part from: https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing?view=aspnetcore-6.0
         */
        public static string encryptPassword(string plaintextPassword, out string saltToStore, string storedSalt = null)
        {
            byte[] salt = new byte[128 / 8];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetNonZeroBytes(salt);
            }
            Console.WriteLine($"Salt: {Convert.ToBase64String(salt)}");

            // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            if (storedSalt != null)
            {
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: plaintextPassword,
                salt: Convert.FromBase64String(storedSalt),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

                string saltString = Convert.ToBase64String(salt);
                byte[] saltBytes = Convert.FromBase64String(saltString);
                saltToStore = storedSalt;
                return hashed;
            }
            else
            {
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: plaintextPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

                saltToStore = Convert.ToBase64String(salt);
                return hashed;
            }
        }

        /// <summary>
        /// User enters their password. That entry, their stored encrypted password, and stored salt are provided here. The entered password 
        /// is encrypted with their stored salt, and that password is compared to the stored password. If they match, true is returned. Otherwise, 
        /// returns false. 
        /// </summary>
        /// <param name="plaintextPassword"></param>
        /// <param name="storedSalt"></param>
        /// <param name="storedHashedPassword"></param>
        /// <returns></returns>
        public static bool checkPassword(string plaintextPassword, string storedSalt, string storedHashedPassword)
        {
            string unusedSalt;
            string passwordToCompare = encryptPassword(plaintextPassword, out unusedSalt, storedSalt);
            if (Equals(passwordToCompare, storedHashedPassword))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

