using System;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using Geonorge.NedlastingApi.V2;
using Kartverket.Geonorge.Download.Services;
using log4net;
using Kartverket.Geonorge.Download.Models;
using System.Collections.Generic;
using System.Web.Http.Cors;
using System.Web.Mvc;
using Kartverket.Geonorge.Utilities;

namespace Kartverket.Geonorge.Download.Controllers.Api.V2
{
    [HandleError]
    [EnableCors("*", "*", "*")]
    public class OrderV2Controller : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IOrderService _orderService;

        public OrderV2Controller(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        ///     Creates new order for data to download
        /// </summary>
        /// <param name="order">OrderType model</param>
        /// <returns>
        ///     OrderReceiptType model with orderreference and a list of files to download if they are prepopulated, otherwise
        ///     the files are delivered via email
        /// </returns>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [System.Web.Http.Route("api/v2/order")]
        [System.Web.Http.HttpPost]
        [ResponseType(typeof(OrderReceiptType))]
        public IHttpActionResult PostOrder(OrderType order)
        {
            try
            {
                string username = SecurityClaim.GetUsername();
                Order savedOrder = _orderService.CreateOrder(order, username);
                return Ok(ConvertToReceipt(savedOrder));
            }
            catch (AccessRestrictionException e)
            {
                Log.Info("Download Access denied", e);
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error("Error API", ex);
                return InternalServerError(ex);
            }
        }

        private OrderReceiptType ConvertToReceipt(Order order)
        {
            return new OrderReceiptType()
            {
                referenceNumber = order.referenceNumber.ToString(),
                email = order.email,
                orderDate = order.orderDate.HasValue ? order.orderDate.Value : DateTime.Now,
                files = ConvertToFiles(order.orderItem)
            };
        }

        private FileType[] ConvertToFiles(List<OrderItem> orderItems)
        {
            var files = new List<FileType>();
            foreach (var item in orderItems)
            {
                files.Add(new FileType()
                {
                    name = item.FileName,
                    downloadUrl = item.DownloadUrl,
                    fileId = item.FileId.ToString(),
                    area = item.Area,
                    coordinates = item.Coordinates,
                    format = item.Format,
                    metadataUuid = item.MetadataUuid,
                    projection = item.Projection,
                    status = item.Status.ToString()
                });
            }
            return files.ToArray();
        }

        [System.Web.Http.Route("api/v2/order/{referenceNumber}")]
        [System.Web.Http.HttpGet]
        [ResponseType(typeof(OrderReceiptType))]
        public IHttpActionResult GetOrder(int referenceNumber)
        {
            // Redirect to web controller if html is requested:
            if (Request.Headers.Accept.First().MediaType.Equals("text/html"))
                return Redirect(Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/order/details/" + referenceNumber);

            var order = _orderService.Find(referenceNumber);
            return order != null ? (IHttpActionResult) Ok(order) : NotFound();
        }
    }
}