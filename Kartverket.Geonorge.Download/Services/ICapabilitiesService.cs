using System.Collections.Generic;
using Geonorge.NedlastingApi.V1;
using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services
{
    public interface ICapabilitiesService
    {
        CapabilitiesType GetCapabilities(string metadataUuid, string apiVersionId);
        Dataset GetDataset(string metadataUuid);
        List<ProjectionType> GetProjections(string metadataUuid);
        List<AreaType> GetAreas(string metadataUuid);
        List<FormatType> GetFormats(string metadataUuid);
    }
}
