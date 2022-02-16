using System;
using System.Collections.Generic;
using System.Net.Http;
using Geonorge.NedlastingApi.V3;
using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services
{
    public interface ICapabilitiesService
    {
        CapabilitiesType GetCapabilities(string metadataUuid);
        Dataset GetDataset(string metadataUuid);
        List<ProjectionType> GetProjections(string metadataUuid);
        List<AreaType> GetAreas(string metadataUuid, HttpRequestMessage request = null);
        List<FormatType> GetFormats(string metadataUuid);
        void SaveClipperFile(Guid id, string url, bool valid, string message);
    }
}
