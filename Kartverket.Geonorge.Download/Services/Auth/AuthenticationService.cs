using System.Net.Http;
using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services.Auth
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IBasicAuthenticationService _basicAuthenticationService;
        private readonly IBaatAuthenticationService _baatAuthenticationService;

        public AuthenticationService(IBaatAuthenticationService baatAuthenticationService,
            IBasicAuthenticationService basicAuthenticationService)
        {
            _baatAuthenticationService = baatAuthenticationService;
            _basicAuthenticationService = basicAuthenticationService;
        }

        /// <summary>
        ///     Gets username of the authenticated user. User can be authenticated with SAML or basic auth
        /// </summary>
        /// <returns>username of authenticated user or null if not authenticated</returns>
        public AuthenticatedUser GetAuthenticatedUser(HttpRequestMessage requestMessage)
        {
            AuthenticatedUser authenticatedUser = _baatAuthenticationService.GetAuthenticatedUser();

            if (authenticatedUser == null)
                authenticatedUser = _basicAuthenticationService.GetAuthenticatedUsername(requestMessage);

            return authenticatedUser;
        }
    }
}