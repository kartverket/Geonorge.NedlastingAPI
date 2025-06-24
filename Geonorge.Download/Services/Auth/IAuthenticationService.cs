using Geonorge.Download.Models;

namespace Geonorge.Download.Services.Auth
{
    public interface IAuthenticationService
    {
        AuthenticatedUser GetAuthenticatedUser(HttpRequest request);
    }
}