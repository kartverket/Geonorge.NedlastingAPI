using Geonorge.NedlastingApi.V3;
using Geonorge.Download.Models;

namespace Geonorge.Download.Services.Interfaces
{
    public interface IRegisterFetcher
    {
        List<AreaType> GetAreas();
        AreaType GetArea(string type, string code);
        List<ProjectionType> GetProjections();
        ProjectionType GetProjection(string code);
        Organization GetOrganization(string organizationNumber);
    }
}