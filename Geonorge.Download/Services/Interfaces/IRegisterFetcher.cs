using Geonorge.NedlastingApi.V1;
using Geonorge.Download.Models;
using System.Collections.Generic;

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