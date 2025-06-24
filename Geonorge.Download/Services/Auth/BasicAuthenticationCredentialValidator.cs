using Geonorge.Download.Models;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Geonorge.Download.Services.Auth
{
    public class BasicAuthenticationCredentialValidator(ILogger<BasicAuthenticationCredentialValidator> logger, DownloadContext downloadContext) : IBasicAuthenticationCredentialValidator
    {
        public bool ValidCredentials(Credentials credentials)
        {
            var account = downloadContext.MachineAccounts.Find(credentials.Username);

            if (account != null)
            {
                // TODO: Replace with a more secure password hashing algorithm
                if (VerifyHashedPassword(account.Passsword, credentials.Password))
                {
                    return true;
                }

                logger.LogInformation($"Basic authentication failed, bad password for username [{credentials.Username}].");
            }
            else
            {
                logger.LogInformation($"Basic authentication failed: username [{credentials.Username}] does not exist.");
            }

            return false;
        }

        public static bool VerifyHashedPassword(string hashedPassword, string password, int iterationCount = 10000)
        {
            if (hashedPassword == null)
            {
                return false;
            }

            if (password == null)
            {
                throw new ArgumentNullException("password");
            }

            byte[] array = Convert.FromBase64String(hashedPassword);
            if (array.Length != 49 || array[0] != 0)
            {
                return false;
            }

            byte[] array2 = new byte[16];
            Buffer.BlockCopy(array, 1, array2, 0, 16);
            byte[] array3 = new byte[32];
            Buffer.BlockCopy(array, 17, array3, 0, 32);
            byte[] bytes;
            using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, array2, iterationCount))
            {
                bytes = rfc2898DeriveBytes.GetBytes(32);
            }

            return ByteArraysEqual(array3, bytes);
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (a == b)
            {
                return true;
            }

            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            bool flag = true;
            for (int i = 0; i < a.Length; i++)
            {
                flag &= a[i] == b[i];
            }

            return flag;
        }
    }
}