using Kartverket.Geonorge.Download.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Kartverket.Geonorge.Download.Controllers
{
    public class OrderController : ApiController
    {

        /// <summary>
        /// Post order
        /// </summary>
        [Route("api/order")]
        public OrderReceiptType Post(object order)
        {
            OrderType o = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderType>(order.ToString());

            return new OrderService().Order(o);
        }
        //public OrderReceiptType Post(OrderType order)
        //{
        //    return new OrderService().Order(order);
        //}

    }
}
