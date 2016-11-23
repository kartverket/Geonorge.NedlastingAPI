using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services
{
    public interface IUpdateFileStatusService
    {
        void UpdateFileStatus(UpdateFileStatusInformation statusInfo);
    }
}