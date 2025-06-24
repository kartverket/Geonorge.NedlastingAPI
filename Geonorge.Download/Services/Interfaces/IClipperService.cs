using System.Collections.Generic;
using Geonorge.NedlastingApi.V3;
using Geonorge.Download.Models;

namespace Geonorge.Download.Services.Interfaces
{
    public interface IClipperService
    {
        List<OrderItem> GetClippableOrderItems(OrderType incomingOrder, AuthenticatedUser authenticatedUser = null, List<Eiendom> eiendoms = null);
        void SendClippingRequests(List<OrderItem> orderItems, string email);
    }
}