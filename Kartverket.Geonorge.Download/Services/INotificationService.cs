using Kartverket.Geonorge.Download.Models;
using System.Collections.Generic;

namespace Kartverket.Geonorge.Download.Services
{
    public interface INotificationService
    {
        void SendReadyForDownloadNotification(OrderItem orderItem);
        void SendReadyForDownloadBundleNotification(Order order);
        void SendOrderInfoNotification(Order order, List<OrderItem> clippableOrderItems);
        void SendOrderStatusNotification(Order order);
        void SendOrderStatusNotificationNotDeliverable(Order order);
    }
}
