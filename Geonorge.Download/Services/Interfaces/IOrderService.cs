using Geonorge.NedlastingApi.V3;
using Geonorge.Download.Models;
using Geonorge.Download.Models.Api.Internal;
using System.Security.Claims;

namespace Geonorge.Download.Services.Interfaces
{
    public interface IOrderService
    {
        Order CreateOrder(OrderType incomingOrder, ClaimsPrincipal principal);

        void UpdateFileStatus(UpdateFileStatusInformation updateFileStatusInformation);

        Order Find(string orderUuid);

        void CheckAccessRestrictions(Order order, ClaimsPrincipal principal);

        OrderItem FindOrderItem(string fileId);

        void UpdateOrder(Order order, OrderType incomingOrder);
        void UpdateOrderStatus(UpdateOrderStatusRequest orderStatus);
        void AddOrderUsage(DownloadUsage usage);
        void SendStatusNotification();
        void SendStatusNotificationNotDeliverable();
        void CheckPackageSize(Order order);
    }
}
