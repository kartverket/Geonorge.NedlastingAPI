using Kartverket.Geonorge.Download.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Kartverket.Geonorge.Download.Controllers
{
    public class OrderController : ApiController
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
      
        /// <summary>
        /// Creates new order for data to download
        /// </summary>
        /// <param name="order">OrderType model</param>
        /// <returns>OrderReceiptType model with orderreference and a list of files to download if they are prepopulated, otherwise the files are delivered via email</returns>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("api/order")]
        [HttpPost]
        [ResponseType(typeof(OrderReceiptType))]
        public IHttpActionResult PostOrder(OrderType order)
        {
            try
            { 
                OrderReceiptType orderrec = new OrderService().Order(order);
                if (orderrec == null)
                {
                    return NotFound();
                }
                return Ok(orderrec);
            }
            catch (Exception ex)
            {
                Log.Error("Error API", ex);
                return InternalServerError(ex);  
            }
        }
       

    }
}
