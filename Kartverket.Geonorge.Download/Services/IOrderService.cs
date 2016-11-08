using Geonorge.NedlastingApi.V2;
using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services
{
    public interface IOrderService
    {
        Order CreateOrder(OrderType incomingOrder, string username);

        void UpdateFileStatus(UpdateFileStatusInformation updateFileStatusInformation);

        OrderReceiptType Find(int referenceNumber);

    }
}