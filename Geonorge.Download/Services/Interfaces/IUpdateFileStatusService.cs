using Geonorge.Download.Models;

namespace Geonorge.Download.Services.Interfaces
{
    public interface IUpdateFileStatusService
    {
        void UpdateFileStatus(UpdateFileStatusInformation statusInfo);
    }
}