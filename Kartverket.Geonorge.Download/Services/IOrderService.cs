using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services
{
    public interface IOrderService
    {
        void UpdateFileStatus(UpdateFileStatusInformation updateFileStatusInformation);
    }
}