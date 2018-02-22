using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Models.Api.Internal;

namespace Kartverket.Geonorge.Download.Services
{
    public interface IUpdateMetadataService
    {
        void UpdateMetadata(UpdateMetadataInformation metadataInfo);
        UpdateMetadataInformation Convert(UpdateMetadataRequest metadata);
    }
}