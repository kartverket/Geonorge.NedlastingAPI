using Geonorge.Download.Models;
using System.Security.Claims;

namespace Geonorge.Download.Services.Interfaces
{
    public interface IFileService
    {
        Task<Dataset> GetDatasetAsync(string metadataUuid);
        Task<Models.File> GetFileAsync(string fileUuid);
        Task<Models.File> GetFileAsync(string fileUuid, string metadataUuid);
        bool HasAccess(Models.File file, ClaimsPrincipal principal);
    }
}
