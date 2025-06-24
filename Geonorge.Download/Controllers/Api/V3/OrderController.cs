using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Geonorge.NedlastingApi.V3;
using Geonorge.Download.Models;
using Geonorge.Download.Services.Auth;
using Microsoft.Net.Http.Headers;
using Asp.Versioning;
using Geonorge.Download.Services.Interfaces;
using Geonorge.Download.Services.Exceptions;

namespace Geonorge.Download.Controllers.Api.V3
{
    /// <summary>
    /// Endpoints managing orders related to data downloads. The endpoints allow users to create, update, and retrieve orders, including handling of file downloads and user authentication. 
    /// </summary>
    [ApiController]
    [ApiVersion("3.0")]
    [Route("api")]
    [Route("api/v{version:apiVersion}")]
    [EnableCors("AllowKartkatalog_2")]
    //[HandleError]
    public class OrderController(ILogger<OrderController> logger, IConfiguration config, IOrderService orderService, IAuthenticationService authenticationService) : ControllerBase
    {
        /// <summary>
        ///     Creates new order for data to download with order reference and a list of files to download if they are prepopulated, otherwise
        ///     the files are delivered via email
        /// </summary>
        /// <param name="order">OrderType model</param>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("order")]
        [Consumes("application/json", "application/xml")]
        [Produces("application/json", "application/xml")]
        [ProducesResponseType(typeof(OrderReceiptType), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult PostOrder([FromBody] OrderType order)
        {
            try
            {
                var savedOrder = orderService.CreateOrder(order, GetAuthenticatedUser());
                orderService.AddOrderUsage(savedOrder.GetDownloadUsage());
                return Ok(ConvertToReceipt(savedOrder, Request));
            }
            catch (AccessRestrictionException e)
            {
                logger.LogInformation(e.Message, e);
                //return Unauthorized();
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
            catch (Exception ex)
            {
                logger.LogError("Error creating new order.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        ///     Get info about files in order
        /// </summary>
        [HttpGet("order/{orderUuid}")]
        [Produces("application/json", "application/xml")]
        [ProducesResponseType(typeof(OrderReceiptType), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetOrder(string orderUuid)
        {
            // Redirect to web controller if html is requested:
            var acceptHeader = Request.Headers[HeaderNames.Accept].ToString();

            if (acceptHeader.StartsWith("text/html", StringComparison.OrdinalIgnoreCase))
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                return Redirect($"{baseUrl}/order/details/{orderUuid}");
            }

            var order = orderService.Find(orderUuid);
            if (order == null)
                return NotFound();

            if (order.ContainsRestrictedDatasets() && !order.BelongsToUser(GetAuthenticatedUser()))
                return Unauthorized();

            return Ok(ConvertToReceipt(order, Request));
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
        [HttpPut("order/{orderUuid}")]
        [Consumes("application/json", "application/xml")]
        [Produces("application/json", "application/xml")]
        [ProducesResponseType(typeof(OrderReceiptType), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateOrder(string orderUuid, [FromBody] OrderType incomingOrder)
        {
            try
            {
                Order order = orderService.Find(orderUuid);
                if (order == null)
                    return NotFound();

                if (order.ContainsRestrictedDatasets() && !order.BelongsToUser(GetAuthenticatedUser()))
                    return Unauthorized();

                orderService.CheckPackageSize(order);

                orderService.UpdateOrder(order, incomingOrder);

                return Ok(ConvertToReceipt(order, Request));
            }
            catch (FileSizeException ex)
            {
                logger.LogInformation(ex.Message, ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (Exception e)
            {
                logger.LogError("Error while updating order.", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        /// <summary>
        ///     Get fileIds for order
        /// </summary>
        /// <returns>
        ///    Http status code 200.
        /// </returns>
        [HttpGet("order/uuidfile/{orderUuid}")]
        public IActionResult GetUuidFile(string orderUuid)
        {
            try
            {
                Order order = orderService.Find(orderUuid);
                if (order == null)
                    return NotFound();

                var fileIds = string.Join(",", order.CollectFileIdsForBundling());
                return Content(fileIds, "text/plain");
            }
            catch (Exception e)
            {
                logger.LogError("Error while getting order uuid file:{e}", e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private AuthenticatedUser GetAuthenticatedUser()
        {
            return authenticationService.GetAuthenticatedUser(HttpContext.Request);
        }

        private OrderReceiptType ConvertToReceipt(Order order, HttpRequest request)
        {
            string downloadBundleUrl = null;
            if (order.DownloadBundleUrl != null)
                downloadBundleUrl = new DownloadUrlBuilder(config).OrderId(order.Uuid).AsBundle();

            var receipt = new OrderReceiptType
            {
                referenceNumber = order.Uuid.ToString(),
                email = order.email,
                orderDate = order.orderDate ?? DateTime.Now,
                files = ConvertToFiles(order.orderItem, order.Uuid),
                downloadAsBundle = order.DownloadAsBundle,
                downloadBundleUrl = downloadBundleUrl,
                _links = new LinkCreator().CreateOrderReceiptLinks(config, request, order.Uuid)
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
                        ? new DownloadUrlBuilder(config).OrderId(orderUuid).FileId(item.Uuid).Build()
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