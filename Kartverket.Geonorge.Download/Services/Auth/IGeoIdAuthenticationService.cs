using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services.Auth
{
    public interface IGeoIdAuthenticationService
    {
        AuthenticatedUser GetAuthenticatedUser();
    }
}