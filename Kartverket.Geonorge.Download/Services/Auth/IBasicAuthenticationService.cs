using System.Net.Http;
using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services.Auth
{
    public interface IBasicAuthenticationService
    {
        AuthenticatedUser GetAuthenticatedUsername(HttpRequestMessage requestMessage);
    }
}