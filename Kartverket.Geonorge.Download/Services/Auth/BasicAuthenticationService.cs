using System;
using System.Net.Http;
using System.Text;
using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services.Auth
{
    public class BasicAuthenticationService : IBasicAuthenticationService
    {
        private readonly IBasicAuthenticationCredentialValidator _credentialValidator;

        public BasicAuthenticationService(IBasicAuthenticationCredentialValidator credentialValidator)
        {
            _credentialValidator = credentialValidator;
        }

        public AuthenticatedUser GetAuthenticatedUsername(HttpRequestMessage requestMessage)
        {
            var credentials = GetCredentials(requestMessage);

            if (credentials != null && Authenticate(credentials))
                return new AuthenticatedUser(credentials.Username, AuthenticationMethod.Basic);

            return null;
        }

        private bool Authenticate(Credentials credentials)
        {
            return _credentialValidator.ValidCredentials(credentials);
        }

        private Credentials GetCredentials(HttpRequestMessage requestMessage)
        {
            var authorization = requestMessage.Headers.Authorization;

            if (authorization == null) return null;
            if (authorization.Scheme != "Basic") return null;
            if (string.IsNullOrEmpty(authorization.Parameter)) return null;

            var credentials = ExtractUsernameAndPassword(authorization.Parameter);

            return credentials;
        }

        private static Credentials ExtractUsernameAndPassword(string authorizationParameter)
        {
            byte[] credentialBytes;

            try
            {
                credentialBytes = Convert.FromBase64String(authorizationParameter);
            }
            catch (FormatException)
            {
                return null;
            }

            // The currently approved HTTP 1.1 specification says characters here are ISO-8859-1.
            // However, the current draft updated specification for HTTP 1.1 indicates this encoding is infrequently
            // used in practice and defines behavior only for ASCII.
            var encoding = Encoding.ASCII;
            // Make a writable copy of the encoding to enable setting a decoder fallback.
            encoding = (Encoding) encoding.Clone();
            // Fail on invalid bytes rather than silently replacing and continuing.
            encoding.DecoderFallback = DecoderFallback.ExceptionFallback;
            string decodedCredentials;

            try
            {
                decodedCredentials = encoding.GetString(credentialBytes);
            }
            catch (DecoderFallbackException)
            {
                return null;
            }

            if (string.IsNullOrEmpty(decodedCredentials)) return null;

            var colonIndex = decodedCredentials.IndexOf(':');

            if (colonIndex == -1) return null;

            var credentials = new Credentials
            {
                Username = decodedCredentials.Substring(0, colonIndex),
                Password = decodedCredentials.Substring(colonIndex + 1)
            };
            return credentials;
        }
    }
}