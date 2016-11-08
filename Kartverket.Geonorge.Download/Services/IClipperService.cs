using System.Collections.Generic;
using Geonorge.NedlastingApi.V2;
using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services
{
    public interface IClipperService
    {
        List<OrderItem> GetClippableOrderItems(OrderType incomingOrder);
    }
}