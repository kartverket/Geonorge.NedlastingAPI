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
    [EnableCors("http://kartkatalog.dev.geonorge.no,https://kartkatalog.dev.geonorge.no,http://kurv.dev.geonorge.no,https://kurv.dev.geonorge.no,https://kartkatalog.test.geonorge.no,https://kartkatalog.geonorge.no", "*", "*", SupportsCredentials = true)]
    public class OrderV2Controller : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IOrderService _orderService;
        private readonly RegisterFetcher _registerFetcher;

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
                Log.Info(e.Message, e);
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
                referenceNumber = order.Uuid.ToString(),
                email = order.email,
                orderDate = order.orderDate ?? DateTime.Now,
                files = ConvertToFiles(order.orderItem, order.Uuid)
            };
        }

        private FileType[] ConvertToFiles(List<OrderItem> orderItems, Guid orderUuid)
        {
            var files = new List<FileType>();
            foreach (var item in orderItems)
            {
                files.Add(new FileType()
                {
                    name = item.FileName,
                    downloadUrl = item.IsReadyForDownload() ? new DownloadUrlBuilder().OrderId(orderUuid).FileId(item.FileId).Build() : null,
                    fileId = item.FileId.ToString(),
                    area =  item.Area ,
                    areaName = item.AreaName,
                    coordinates = item.Coordinates,
                    format = item.Format,
                    metadataUuid = item.MetadataUuid,
                    projection = item.Projection,
                    projectionName = item.ProjectionName,
                    status = item.Status.ToString(),
                    metadataName = item.MetadataName
                });
            }
            return files.ToArray();
        }

        /// <summary>
        ///     Get info about files in order
        /// </summary>
        [System.Web.Http.Route("api/v2/order/{referenceNumber}")]
        [System.Web.Http.HttpGet]
        [ResponseType(typeof(OrderReceiptType))]
        public IHttpActionResult GetOrder(string referenceNumber)
        {
            // Redirect to web controller if html is requested:
            if (Request.Headers.Accept.First().MediaType.Equals("text/html"))
                return Redirect(Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/order/details/" + referenceNumber);

            Order order = _orderService.Find(referenceNumber);
            if (order == null)
                return NotFound();

            if (!order.BelongsToUser(SecurityClaim.GetUsername()))
                return Unauthorized();

            return Ok(ConvertToReceipt(order));
        }
    }
}