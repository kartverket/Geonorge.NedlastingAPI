using Geonorge.NedlastingApi.V3;
using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services
{
    public interface IOrderService
    {
        Order CreateOrder(OrderType incomingOrder, string username);

        void UpdateFileStatus(UpdateFileStatusInformation updateFileStatusInformation);

        Order Find(string orderUuid);

        void CheckAccessRestrictions(Order order, string username);

        OrderItem FindOrderItem(string fileId);

        void UpdateOrder(Order order);
    }
}
