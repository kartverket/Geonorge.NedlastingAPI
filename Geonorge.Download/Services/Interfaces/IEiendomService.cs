using Geonorge.Download.Models;

namespace Geonorge.Download.Services.Interfaces
{
    public interface IEiendomService
    {
        List<Eiendom> GetEiendoms(AuthenticatedUser user);
    }
}
