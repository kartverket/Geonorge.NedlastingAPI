using System.Collections.Generic;
using System.Security.Claims;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Utilities;

namespace Kartverket.Geonorge.Download.Services.Auth
{
    public class BaatAuthentication : IBaatAuthenticationService
    {
        public AuthenticatedUser GetAuthenticatedUser()
        {
            var username = SecurityClaim.GetUsername(); 
            if (string.IsNullOrEmpty(username))
                return null;

            List<string> roles = GetSecurityClaims("role");

            return new AuthenticatedUser(username, AuthenticationMethod.Baat, roles);
        }

        private List<string> GetSecurityClaims(string type)
        {
            var result = new List<string>();
            foreach (var claim in ClaimsPrincipal.Current.Claims)
            {
                if (claim.Type == type && !string.IsNullOrWhiteSpace(claim.Value))
                {
                    result.Add(claim.Value);
                }
            }
            return result;
        }

    }
}