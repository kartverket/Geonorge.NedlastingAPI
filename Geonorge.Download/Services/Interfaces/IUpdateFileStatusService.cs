using Geonorge.Download.Models;

namespace Geonorge.Download.Services.Interfaces
{
    public interface IUpdateFileStatusService
    {
        Task UpdateFileStatus(UpdateFileStatusInformation statusInfo);
    }
}