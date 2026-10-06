using Geonorge.Download.Models;

namespace Geonorge.Download.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendReadyForDownloadNotification(OrderItem orderItem);
        Task SendReadyForDownloadBundleNotification(Order order);
        Task SendOrderInfoNotification(Order order, List<OrderItem> clippableOrderItems);
        Task SendOrderStatusNotification(Order order);
        Task SendOrderStatusNotificationNotDeliverable(Order order);
    }
}
