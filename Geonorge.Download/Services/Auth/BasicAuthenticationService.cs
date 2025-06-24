using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Geonorge.Download.Models;

namespace Geonorge.Download.Services.Auth
{
    public class BasicAuthenticationService(IBasicAuthenticationCredentialValidator credentialValidator, DownloadContext downloadContext) : IBasicAuthenticationService
    {
        public AuthenticatedUser GetAuthenticatedUsername(HttpRequest request)
        {
            var credentials = GetCredentials(request);

            if (credentials != null && Authenticate(credentials)) {
                List<string> roles = GetRolesForUser(credentials.Username);
                UserInfo userInfo = new UserInfo();
                userInfo._roles = roles;
                return new AuthenticatedUser(credentials.Username, AuthenticationMethod.Basic, userInfo);
            }

            return null;
        }

        private List<string> GetRolesForUser(string userName)
        {
            var account = downloadContext.MachineAccounts.Find(userName);

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
            return credentialValidator.ValidCredentials(credentials);
        }

        private Credentials GetCredentials(HttpRequest request)
        {
            if (!request.Headers.TryGetValue("Authorization", out var authHeaderValues))
                return null;

            var rawHeader = authHeaderValues.FirstOrDefault();
            if (string.IsNullOrEmpty(rawHeader))
                return null;

            var authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse(rawHeader);

            if (!string.Equals(authorization.Scheme, "Basic", StringComparison.OrdinalIgnoreCase))
                return null;

            if (string.IsNullOrEmpty(authorization.Parameter))
                return null;

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