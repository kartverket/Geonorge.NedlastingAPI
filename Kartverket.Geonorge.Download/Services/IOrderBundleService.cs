using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services
{
    public interface IOrderBundleService
    {
        void SendToBundling(Order order);
    }
}