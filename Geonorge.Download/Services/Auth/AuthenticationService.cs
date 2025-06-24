using Geonorge.Download.Models;

namespace Geonorge.Download.Services.Auth
{
    public class AuthenticationService(IBasicAuthenticationService basicAuthenticationService, IGeoIdAuthenticationService geoIdAuthenticationService) : IAuthenticationService
    {
        /// <summary>
        ///     Gets username of the authenticated user. User can be authenticated with GeoID or basic auth
        /// </summary>
        /// <returns>username of authenticated user or null if not authenticated</returns>
        public AuthenticatedUser GetAuthenticatedUser(HttpRequest request)
        {
            AuthenticatedUser authenticatedUser = geoIdAuthenticationService.GetAuthenticatedUser(request);

            if (authenticatedUser == null)
                authenticatedUser = basicAuthenticationService.GetAuthenticatedUsername(request);

            return authenticatedUser;
        }
    }
}