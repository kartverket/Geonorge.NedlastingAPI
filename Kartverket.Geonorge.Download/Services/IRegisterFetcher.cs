using Geonorge.NedlastingApi.V1;
using System.Collections.Generic;

namespace Kartverket.Geonorge.Download.Services
{
    public interface IRegisterFetcher
    {
        List<AreaType> GetAreas();
        AreaType GetArea(string type, string code);
        List<ProjectionType> GetProjections();
        ProjectionType GetProjection(string code);
    }
}