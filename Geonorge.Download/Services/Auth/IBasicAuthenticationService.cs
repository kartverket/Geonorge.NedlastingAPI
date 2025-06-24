using System.Net.Http;
using Geonorge.Download.Models;

namespace Geonorge.Download.Services.Auth
{
    public interface IBasicAuthenticationService
    {
        AuthenticatedUser GetAuthenticatedUsername(HttpRequest request);
    }
}