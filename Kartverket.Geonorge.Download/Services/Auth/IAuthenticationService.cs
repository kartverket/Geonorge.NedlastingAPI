using System.Net.Http;
using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services.Auth
{
    public interface IAuthenticationService
    {
        AuthenticatedUser GetAuthenticatedUser(HttpRequestMessage requestMessage);
    }
}