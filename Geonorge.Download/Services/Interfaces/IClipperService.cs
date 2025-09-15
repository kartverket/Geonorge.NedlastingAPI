using Geonorge.Download.Models;
using Geonorge.NedlastingApi.V3;
using System.Security.Claims;

namespace Geonorge.Download.Services.Interfaces
{
    public interface IClipperService
    {
        List<OrderItem> GetClippableOrderItems(OrderType incomingOrder, ClaimsPrincipal principal = null, List<Eiendom> eiendoms = null);
        void SendClippingRequests(List<OrderItem> orderItems, string email);
    }
}