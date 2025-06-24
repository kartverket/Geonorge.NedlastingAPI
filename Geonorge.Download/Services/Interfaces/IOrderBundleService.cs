using Geonorge.Download.Models;

namespace Geonorge.Download.Services.Interfaces
{
    public interface IOrderBundleService
    {
        void SendToBundling(Order order);
    }
}