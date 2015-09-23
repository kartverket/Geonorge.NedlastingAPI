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
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Post order
        /// </summary>
        [Route("api/order")]
        [HttpPost]
        public OrderReceiptType Post(OrderType order)
        {
            try
            { 
                //OrderType o = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderType>(order.ToString());
                return new OrderService().Order(order);
            }
            catch (Exception ex)
            {
                Log.Error("Error API", ex);
                return null;
            }
        }
       

    }
}
