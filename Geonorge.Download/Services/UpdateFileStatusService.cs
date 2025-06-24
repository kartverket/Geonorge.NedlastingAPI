using System.Reflection;
using Geonorge.Download.Models;
using Geonorge.Download.Services.Interfaces;

namespace Geonorge.Download.Services
{
    public class UpdateFileStatusService(ILogger<UpdateFileStatusService> logger, INotificationService notificationService, IOrderService orderService) : IUpdateFileStatusService
    {
        public void UpdateFileStatus(UpdateFileStatusInformation statusInfo)
        {
            OrderItem orderItem = orderService.FindOrderItem(statusInfo.FileId);

            if (orderItem.Status != statusInfo.Status)
            {
                orderService.UpdateFileStatus(statusInfo);
                orderItem = orderService.FindOrderItem(statusInfo.FileId);
                if (IsReadyForDownloadNotification(orderItem))
                    notificationService.SendReadyForDownloadNotification(orderItem);
            }
            else
            {
                logger.LogInformation($"Not updating status of fileId: {statusInfo.FileId}, status is already {statusInfo.Status}");
            }
        }

        private bool IsReadyForDownloadNotification(OrderItem orderItem)
        {
            logger.LogInformation($"Check IsReadyForDownloadNotification orderItemId: {orderItem.Id}");

            bool waitingForProcessing = false;
            foreach (var item in orderItem.Order.orderItem)
            {
                logger.LogInformation($"Check status orderitem: {item.Id}, status: {item.Status}");
                if (item.Status == OrderItemStatus.WaitingForProcessing)
                    waitingForProcessing = true;
            }

            if (waitingForProcessing) {
                return false;
            }
            else {
                return true;
            }
        }
    }
}