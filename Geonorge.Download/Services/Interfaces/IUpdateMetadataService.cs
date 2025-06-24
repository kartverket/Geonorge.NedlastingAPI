using Geonorge.Download.Models;
using Geonorge.Download.Models.Api.Internal;

namespace Geonorge.Download.Services.Interfaces
{
    public interface IUpdateMetadataService
    {
        void UpdateMetadata(UpdateMetadataInformation metadataInfo);
        UpdateMetadataInformation Convert(UpdateMetadataRequest metadata);
    }
}