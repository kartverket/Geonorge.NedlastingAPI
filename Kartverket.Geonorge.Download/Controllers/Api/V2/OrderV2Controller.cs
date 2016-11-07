using System;
using System.Web.Http;
using System.Web.Http.Description;
using Geonorge.NedlastingApi.V2;
using Kartverket.Geonorge.Download.Services;

namespace Kartverket.Geonorge.Download.Controllers.Api.V2
{
    [ApiExplorerSettings(IgnoreApi = true)] // undocumented until version 2 is ready to be released
    public class OrderV2Controller : ApiController
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
      
        /// <summary>
        /// Creates new order for data to download
        /// </summary>
        /// <param name="order">OrderType model</param>
        /// <returns>OrderReceiptType model with orderreference and a list of files to download if they are prepopulated, otherwise the files are delivered via email</returns>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("api/v2/order")]
        [HttpPost]
        [ResponseType(typeof(OrderReceiptType))]
        public IHttpActionResult PostOrder(OrderType order)
        {
            try
            { 
                // this is version 1 of OrderService - todo implement this for version 2
                /*
                OrderReceiptType orderrec = new OrderService().Order(order);
                if (orderrec == null)
                {
                    return NotFound();
                }
                return Ok(orderrec);
                */

                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error("Error API", ex);
                return InternalServerError(ex);  
            }
        }
       

    }
}
