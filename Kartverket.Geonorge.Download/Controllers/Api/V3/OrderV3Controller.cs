﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Web.Mvc;
using Geonorge.NedlastingApi.V3;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;
using Kartverket.Geonorge.Download.Services.Auth;
using log4net;
using Microsoft.Web.Http;

namespace Kartverket.Geonorge.Download.Controllers.Api.V3
{
    [ApiVersion("3.0")]
    [EnableCors(
        "http://kartkatalog.dev.geonorge.no,https://kartkatalog.dev.geonorge.no,https://kartkatalog.test.geonorge.no,https://kartkatalog.geonorge.no,https://localhost:44355,http://localhost:50081",
        "*", "*", SupportsCredentials = true)]
    [HandleError]
    public class OrderV3Controller : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IOrderService _orderService;
        private readonly IAuthenticationService _authenticationService;

        public OrderV3Controller(IOrderService orderService, IAuthenticationService authenticationService)
        {
            _orderService = orderService;
            _authenticationService = authenticationService;
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
        [System.Web.Http.Route("api/order")]
        [System.Web.Http.HttpPost]
        [ResponseType(typeof(OrderReceiptType))]
        public IHttpActionResult PostOrder([FromBody] OrderType order)
        {
            try
            {
                var savedOrder = _orderService.CreateOrder(order, GetAuthenticatedUser());
                _orderService.AddOrderUsage(savedOrder.GetDownloadUsage());
                return Ok(ConvertToReceipt(savedOrder));
            }
            catch (AccessRestrictionException e)
            {
                Log.Info(e.Message, e);
                //return Unauthorized();
                return Content(System.Net.HttpStatusCode.InternalServerError, e.Message);
            }
            catch (Exception ex)
            {
                Log.Error("Error creating new order.", ex);
                return InternalServerError(ex);
            }
        }
        
        /// <summary>
        ///     Get info about files in order
        /// </summary>
        [System.Web.Http.Route("api/order/{orderUuid}")]
        [System.Web.Http.HttpGet]
        [ResponseType(typeof(OrderReceiptType))]
        public IHttpActionResult GetOrder(string orderUuid)
        {
            // Redirect to web controller if html is requested:
            if (Request.Headers.Accept.First().MediaType.Equals("text/html"))
                return Redirect(Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/order/details/" + orderUuid);

            var order = _orderService.Find(orderUuid);
            if (order == null)
                return NotFound();

            if (order.ContainsRestrictedDatasets() && !order.BelongsToUser(GetAuthenticatedUser()))
                return Unauthorized();

            return Ok(ConvertToReceipt(order));
        }

        /// <summary>
        ///     Updates order with info to send to bundling
        /// </summary>
        /// <param name="orderUuid">Uuid for order to update</param>
        /// <param name="incomingOrder">
        /// Currently only these fields are updated:
        /// * email
        /// * downloadAsBundle , sample: { "downloadAsBundle": true,"email": "user@email.no" }</param>
        /// <returns>
        ///    Http status code 200 if updated ok.
        /// </returns>
        [System.Web.Http.HttpPut]
        [System.Web.Http.Route("api/order/{orderUuid}")]
        public IHttpActionResult UpdateOrder(string orderUuid, [FromBody] OrderType incomingOrder)
        {
            try
            {
                Order order = _orderService.Find(orderUuid);
                if (order == null)
                    return NotFound();

                if (order.ContainsRestrictedDatasets() && !order.BelongsToUser(GetAuthenticatedUser()))
                    return Unauthorized();

                _orderService.CheckPackageSize(order);

                _orderService.UpdateOrder(order, incomingOrder);

                return Ok(ConvertToReceipt(order));
            }
            catch (FileSizeException ex)
            {
                Log.Info(ex.Message, ex);
                return Content(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
            catch (Exception e)
            {
                Log.Error("Error while updating order.", e);
                return InternalServerError(e);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        /// <summary>
        ///     Get fileIds for order
        /// </summary>
        /// <returns>
        ///    Http status code 200.
        /// </returns>
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/order/uuidfile/{orderUuid}")]
        public HttpResponseMessage GetUuidFile(string orderUuid)
        {
            try
            {
                Order order = _orderService.Find(orderUuid);
                if (order == null)
                    return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);

                var fileIds = string.Join(",", order.CollectFileIdsForBundling());

                var response = new HttpResponseMessage();
                response.Content = new StringContent(fileIds);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                return response;
            }
            catch (Exception e)
            {
                Log.Error("Error while getting order uuid file.", e);
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
            }
        }

        private AuthenticatedUser GetAuthenticatedUser()
        {
            return _authenticationService.GetAuthenticatedUser(ControllerContext.Request);
        }

        private OrderReceiptType ConvertToReceipt(Order order)
        {
            string downloadBundleUrl = null;
            if (order.DownloadBundleUrl != null)
                downloadBundleUrl = new DownloadUrlBuilder().OrderId(order.Uuid).AsBundle();

            var receipt = new OrderReceiptType
            {
                referenceNumber = order.Uuid.ToString(),
                email = order.email,
                orderDate = order.orderDate ?? DateTime.Now,
                files = ConvertToFiles(order.orderItem, order.Uuid),
                downloadAsBundle = order.DownloadAsBundle,
                downloadBundleUrl = downloadBundleUrl,
                _links = new LinkCreator().CreateOrderReceiptLinks(order.Uuid)
            };
            return receipt;
        }

        private FileType[] ConvertToFiles(List<OrderItem> orderItems, Guid orderUuid)
        {
            var files = new List<FileType>();
            foreach (var item in orderItems)
                files.Add(new FileType
                {
                    name = item.FileName,
                    downloadUrl = item.IsReadyForDownload()
                        ? new DownloadUrlBuilder().OrderId(orderUuid).FileId(item.Uuid).Build()
                        : null,
                    fileId = item.Uuid.ToString(),
                    area = item.Area,
                    areaName = item.AreaName,
                    coordinates = item.Coordinates,
                    format = item.Format,
                    metadataUuid = item.MetadataUuid,
                    projection = item.Projection,
                    projectionName = item.ProjectionName,
                    status = item.Status.ToString(),
                    metadataName = item.MetadataName
                });
            return files.ToArray();
        }
    }
}