
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace Geonorge.Download.Services.Auth
{
    public interface IHMACService
    {
        string Hash(string pid);
        bool Verify(string storedIdentifier, string pid);
    }

    /// <summary>
    /// Thrown when HMAC key is unavailable or cannot be used.
    /// </summary>
    public class HMACUnavailableException(
        string message,
        Exception? inner = null)
        : Exception(message, inner);

    public class HMACService(IConfiguration config) : IHMACService
    {
        private const string Scheme = "pid-hmac";
        private const string Version = "v1";
        private const string CurrentKeyId = "k2026-01";

        public string Hash(string pid)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pid);

            byte[] secretKey = GetSecretKey();
            byte[] digest = ComputeDigest(pid, secretKey);

            return $"{Scheme}:{Version}:{CurrentKeyId}:" +
                WebEncoders.Base64UrlEncode(digest);
        }

        public bool Verify(string storedIdentifier, string pid)
        {
            if (string.IsNullOrWhiteSpace(storedIdentifier) ||
                string.IsNullOrWhiteSpace(pid))
            {
                return false;
            }

            // Expected format:
            // pid-hmac:v1:k2026-01:<base64url-digest>
            string[] parts = storedIdentifier.Split(':');

            if (parts.Length != 4 ||
                parts[0] != Scheme ||
                parts[1] != Version)
            {
                return false;
            }

            string keyId = parts[2];

            // TODO: Key rotation
            // Resolve the appropriate secret key using keyId.
            // For now, only the current key is supported.
            if (keyId != CurrentKeyId)
            {
                return false;
            }

            byte[] storedDigest;

            try
            {
                storedDigest = WebEncoders.Base64UrlDecode(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            // HMAC-SHA256 produces exactly 32 bytes.
            if (storedDigest.Length != 32)
            {
                return false;
            }

            byte[] secretKey = GetSecretKey();
            byte[] calculatedDigest = ComputeDigest(pid, secretKey);

            // Compare digests without leaking information
            // through content-dependent comparison timing.
            return CryptographicOperations.FixedTimeEquals(
                storedDigest,
                calculatedDigest);
        }

        private static byte[] ComputeDigest(
            string pid,
            byte[] secretKey)
        {
            byte[] input = Encoding.ASCII.GetBytes(
                $"download:order-owner:{Version}:{pid}");

            return HMACSHA256.HashData(secretKey, input);
        }

        private byte[] GetSecretKey()
        {
            string? key = config["HMAC_SECRET_KEY"];

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new HMACUnavailableException(
                    "HMAC secret key is not configured.");
            }

            // TODO: Key rotation
            // Accept a keyId parameter and retrieve the matching
            // historical key from a key store/secret manager.
            //
            // Important: Keep the same key encoding for existing
            // orders, otherwise their digests will no longer match.

            return Encoding.UTF8.GetBytes(key);
        }
    }
}
