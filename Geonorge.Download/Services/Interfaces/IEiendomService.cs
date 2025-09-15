using Geonorge.Download.Models;
using System.Security.Claims;

namespace Geonorge.Download.Services.Interfaces
{
    public interface IEiendomService
    {
        List<Eiendom> GetEiendoms(ClaimsPrincipal user);
    }
}
