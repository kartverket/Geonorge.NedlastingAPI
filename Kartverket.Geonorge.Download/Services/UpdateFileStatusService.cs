using System.Reflection;
using Kartverket.Geonorge.Download.Models;
using log4net;

namespace Kartverket.Geonorge.Download.Services
{
    public class UpdateFileStatusService : IUpdateFileStatusService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly INotificationService _notificationService;

        private readonly IOrderService _orderService;

        public UpdateFileStatusService(IOrderService orderService, INotificationService notificationService)
        {
            _orderService = orderService;
            _notificationService = notificationService;
        }

        public void UpdateFileStatus(UpdateFileStatusInformation statusInfo)
        {
            OrderItem orderItem = _orderService.FindOrderItem(statusInfo.FileId);

            if (orderItem.Status != statusInfo.Status)
            {
                _orderService.UpdateFileStatus(statusInfo);
                orderItem = _orderService.FindOrderItem(statusInfo.FileId);
                if (IsReadyForDownloadNotification(orderItem))
                    _notificationService.SendReadyForDownloadNotification(orderItem);
            }
            else
            {
                Log.Info($"Not updating status of fileId: {statusInfo.FileId}, status is already {statusInfo.Status}");
            }
        }

        private bool IsReadyForDownloadNotification(OrderItem orderItem)
        {
            Log.Info($"Check IsReadyForDownloadNotification orderItemId: {orderItem.Id}");
            //var orderInfo = _orderService.FindOrderItem(orderItem.Uuid.ToString());
            //Log.Info($"Check IsReadyForDownloadNotification order uuid: {orderItem.Uuid.ToString()}");
            //var orderItems = orderInfo.Order.orderItem;
            bool waitingForProcessing = false;
            foreach (var item in orderItem.Order.orderItem)
            {
                Log.Info($"Check status orderitem: {item.Id}, status: {item.Status}");
                if (item.Status == OrderItemStatus.WaitingForProcessing)
                    waitingForProcessing = true;
            }

            if (waitingForProcessing) {
                Log.Info("IsReadyForDownloadNotification orderitem: false");
                return false;
            }
            else {
                Log.Info("IsReadyForDownloadNotification orderitem: true");
                return true;
            }
        }
    }
}