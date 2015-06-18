using Kartverket.Geonorge.Download.Services;
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
        public OrderReceiptType Post([FromBody]OrderType order)
        {
            return new OrderService().Order(order);
        }

    }
}
