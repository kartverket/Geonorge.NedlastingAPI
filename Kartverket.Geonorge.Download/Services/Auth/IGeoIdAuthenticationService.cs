using System;
using System.Net.Http;
using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services.Auth
{
    public interface IGeoIdAuthenticationService
    {
        /// <summary>
        /// Get current logged in user. 
        /// </summary>
        /// <returns></returns>
        AuthenticatedUser GetAuthenticatedUser();

        /// <summary>
        /// Get current logged in user from bearer access token
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        AuthenticatedUser GetAuthenticatedUser(HttpRequestMessage requestMessage);
    }
}