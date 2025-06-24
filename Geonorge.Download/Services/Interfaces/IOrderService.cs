using Geonorge.NedlastingApi.V3;
using Geonorge.Download.Models;
using Geonorge.Download.Models.Api.Internal;

namespace Geonorge.Download.Services.Interfaces
{
    public interface IOrderService
    {
        Order CreateOrder(OrderType incomingOrder, AuthenticatedUser authenticatedUser);

        void UpdateFileStatus(UpdateFileStatusInformation updateFileStatusInformation);

        Order Find(string orderUuid);

        void CheckAccessRestrictions(Order order, AuthenticatedUser authenticatedUser);

        OrderItem FindOrderItem(string fileId);

        void UpdateOrder(Order order, OrderType incomingOrder);
        void UpdateOrderStatus(UpdateOrderStatusRequest orderStatus);
        void AddOrderUsage(DownloadUsage usage);
        void SendStatusNotification();
        void SendStatusNotificationNotDeliverable();
        void CheckPackageSize(Order order);
    }
}
