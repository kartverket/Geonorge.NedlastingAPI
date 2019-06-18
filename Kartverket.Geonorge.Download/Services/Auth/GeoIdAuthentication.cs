using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Geonorge.AuthLib.Common;
using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services.Auth
{
    public class GeoIdAuthentication : IGeoIdAuthenticationService
    {
        public AuthenticatedUser GetAuthenticatedUser()
        {
            var username = ClaimsPrincipal.Current.GetUsername();

            if (string.IsNullOrEmpty(username))
                return null;

            IEnumerable<Claim> roles = ClaimsPrincipal.Current.FindAll(GeonorgeAuthorizationService.ClaimIdentifierRole);
            var rolesAsList = roles.Select(r => r.Value).ToList();

            return new AuthenticatedUser(username, AuthenticationMethod.GeoId, rolesAsList);
        }
        
    }
}