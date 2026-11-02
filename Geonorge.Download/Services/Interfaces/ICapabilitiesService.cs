using Geonorge.NedlastingApi.V3;
using Geonorge.Download.Models;
using System.Security.Claims;

namespace Geonorge.Download.Services.Interfaces
{
    public interface ICapabilitiesService
    {
        CapabilitiesType GetCapabilities(HttpRequest request, string metadataUuid);
        Dataset GetDataset(string metadataUuid);
        List<ProjectionType> GetProjections(string metadataUuid);
        List<AreaType> GetAreas(string metadataUuid, ClaimsPrincipal principal);

        List<AreaType> GetAllAreas(string metadataUuid);
        List<FormatType> GetFormats(string metadataUuid);
        void SaveClipperFile(Guid id, string url, bool valid, string message);
    }
}
