using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services.Auth
{
    public class BasicAuthenticationService : IBasicAuthenticationService
    {
        private readonly IBasicAuthenticationCredentialValidator _credentialValidator;
        private readonly DownloadContext _dbContext;

        public BasicAuthenticationService(IBasicAuthenticationCredentialValidator credentialValidator, DownloadContext dbContext)
        {
            _credentialValidator = credentialValidator;
            _dbContext = dbContext;
        }

        public AuthenticatedUser GetAuthenticatedUsername(HttpRequestMessage requestMessage)
        {
            var credentials = GetCredentials(requestMessage);

            if (credentials != null && Authenticate(credentials)) {
                List<string> roles = GetRolesForUser(credentials.Username);
                return new AuthenticatedUser(credentials.Username, AuthenticationMethod.Basic, roles);
            }

            return null;
        }

        private List<string> GetRolesForUser(string userName)
        {
            var account = _dbContext.MachineAccounts.Find(userName);

            if (account != null)
            {
                var roles = account.Roles;
                if (!string.IsNullOrEmpty(roles))
                {
                    var rolesForUser = roles.Split(',').ToList();
                    return rolesForUser;
                }
            }

            return new List<string>();
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