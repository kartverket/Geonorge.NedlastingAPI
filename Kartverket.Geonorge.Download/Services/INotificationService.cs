using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services
{
    public interface INotificationService
    {
        void SendReadyForDownloadNotification(OrderItem orderItem);
        void SendReadyForDownloadBundleNotification(Order order);
    }
}
