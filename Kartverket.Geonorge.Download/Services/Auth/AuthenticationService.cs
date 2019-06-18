using System.Net.Http;
using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services.Auth
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IBasicAuthenticationService _basicAuthenticationService;
        private readonly IGeoIdAuthenticationService _geoIdAuthenticationService;

        public AuthenticationService(IGeoIdAuthenticationService geoIdAuthenticationService,
            IBasicAuthenticationService basicAuthenticationService)
        {
            _geoIdAuthenticationService = geoIdAuthenticationService;
            _basicAuthenticationService = basicAuthenticationService;
        }

        /// <summary>
        ///     Gets username of the authenticated user. User can be authenticated with GeoID or basic auth
        /// </summary>
        /// <returns>username of authenticated user or null if not authenticated</returns>
        public AuthenticatedUser GetAuthenticatedUser(HttpRequestMessage requestMessage)
        {
            AuthenticatedUser authenticatedUser = _geoIdAuthenticationService.GetAuthenticatedUser();

            if (authenticatedUser == null)
                authenticatedUser = _basicAuthenticationService.GetAuthenticatedUsername(requestMessage);

            return authenticatedUser;
        }
    }
}