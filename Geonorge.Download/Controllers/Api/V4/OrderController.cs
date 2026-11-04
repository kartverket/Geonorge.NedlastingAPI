using Asp.Versioning;
using Geonorge.Download.Models;
using Geonorge.Download.Services.Auth;
using Geonorge.Download.Services.Exceptions;
using Geonorge.Download.Services.Interfaces;
using Geonorge.NedlastingApi.V3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace Geonorge.Download.Controllers.Api.V4
{
    /// <summary>
    /// Endpoints managing orders related to data downloads. The endpoints allow users to create, update, and retrieve orders, including handling of file downloads and user authentication. 
    /// </summary>
    [ApiController]
    [ApiVersion("4.0")]
    [ApiVersionHeader("4.0")]
    [Route("api/v{version:apiVersion}")]
    [EnableCors("AllowKartkatalog_2")]
    [AllowAnonymous]
    //[HandleError]
    public class OrderController(ILogger<OrderController> logger, IConfiguration config, IOrderService orderService, IHttpClientFactory httpClientFactory, IOpaClient opaClient) : ControllerBase
    {
        /// <summary>
        /// Creates new order for data to download with order reference and a list of files to download if they are prepopulated, otherwise
        /// the files are delivered via email
        /// </summary>
        /// <param name="order">OrderType model</param>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("order")]
        [Consumes("application/json", "application/xml")]
        [Produces("application/json", "application/xml")]
        [ProducesResponseType(typeof(OrderReceiptType), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PostOrder([FromBody] OrderType order)
        {
            try
            {
                var savedOrder = await orderService.CreateOrderOpenData(order);
                orderService.AddOrderUsage(savedOrder.GetDownloadUsage());
                return Ok(ConvertToReceipt(savedOrder, Request));
            }
            catch (AccessRestrictionException e)
            {
                logger.LogInformation(e.Message, e);
                //return Unauthorized();
                return StatusCode(StatusCodes.Status403Forbidden, "No restricted datasets allowed");
            }

            catch (InvalidGeometryException e)
            {
                logger.LogInformation(e.Message, e);
                return StatusCode(StatusCodes.Status400BadRequest, $"Invalid geometry for dataset, {e.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError("Error creating new order.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Creates an order containing restricted datasets. Requires a valid TokenX access token
        /// and Altinn access to download datasets on behalf of an organisation.
        /// </summary>
        /// <param name="order">OrderType model</param>
        /// <response code="401">Missing or invalid access token</response>
        /// <response code="403">User is not permitted to order</response>
        /// <response code="503">Authorization service unavailable</response>
        [HttpPost("order-restricted")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(OrderReceiptType), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> PostRestrictedOrder([FromBody] OrderType order)
        {
            if (!AuthenticationHeaderValue.TryParse(Request.Headers.Authorization.ToString(), out var auth)
                || !string.Equals(auth.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(auth.Parameter))
            {
                return Unauthorized();
            }

            var opaBaseUrl = (config["OPA_URL"] ?? "http://localhost:3010").TrimEnd('/');
            var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            OpaDecision? decision;
            try
            {
                using var response = await client.PostAsJsonAsync(
                    $"{opaBaseUrl}/v1/data/download/decision",
                    new { input = new { token = auth.Parameter } });

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError("OPA returned {StatusCode}", response.StatusCode);
                    return StatusCode(StatusCodes.Status503ServiceUnavailable);
                }

                decision = await response.Content.ReadFromJsonAsync<OpaDecision>();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                logger.LogError(ex, "OPA request failed");
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            if (decision?.Result is not { ValidToken: true } result)
                return Unauthorized();

            if (!result.CanOrder)
                return StatusCode(StatusCodes.Status403Forbidden);

            // TODO: create the order
            return Ok();
        }

        private sealed record OpaDecision(OrderDecision? Result);

        private sealed record OrderDecision(
            [property: JsonPropertyName("valid_token")] bool ValidToken,
            [property: JsonPropertyName("can_order")] bool CanOrder);

        /// <summary>
        /// Get info about files in order
        /// </summary>
        [HttpGet("order/{orderUuid}")]
        [Produces("application/json", "application/xml")]
        [ProducesResponseType(typeof(OrderReceiptType), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetOrder(string orderUuid)
        {
            // Redirect to web controller if html is requested:
            var acceptHeader = Request.Headers.Accept.ToString();

            if (acceptHeader.StartsWith("text/html", StringComparison.OrdinalIgnoreCase))
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                return Redirect($"{baseUrl}/order/details/{orderUuid}");
            }

            var order = orderService.Find(orderUuid);
            if (order == null)
                return NotFound();

            if (order.ContainsRestrictedDatasets())
                return Forbid();

            return Ok(ConvertToReceipt(order, Request));
        }

        /// <summary>
        /// Updates order with info to send to bundling
        /// </summary>
        /// <param name="orderUuid">Uuid for order to update</param>
        /// <param name="incomingOrder">
        /// Currently only these fields are updated:
        /// * email
        /// * downloadAsBundle , sample: { "downloadAsBundle": true,"email": "user@email.no" }</param>
        /// <returns>
        /// Http status code 200 if updated ok.
        /// </returns>
        [HttpPut("order/{orderUuid}")]
        [Consumes("application/json", "application/xml")]
        [Produces("application/json", "application/xml")]
        [ProducesResponseType(typeof(OrderReceiptType), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateOrder(string orderUuid, [FromBody] OrderType incomingOrder)
        {
            try
            {
                Order order = orderService.Find(orderUuid);
                if (order == null)
                    return NotFound();

                if (order.ContainsRestrictedDatasets())
                    return Forbid();

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
        /// Get fileIds for order
        /// </summary>
        /// <returns>
        /// Http status code 200.
        /// </returns>
        ///         
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("order/uuidfile/{orderUuid}")]
        public IActionResult GetUuidFile(string orderUuid)
        {
            try
            {
                Order order = orderService.Find(orderUuid);
                if (order == null)
                    return NotFound();

                if (order.ContainsRestrictedDatasets())
                    return Forbid();

                var fileIds = string.Join(",", order.CollectFileIdsForBundling());
                return Content(fileIds, "text/plain");
            }
            catch (Exception e)
            {
                logger.LogError("Error while getting order uuid file:{e}", e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
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
                orderDate = order.orderDate ?? DateTime.UtcNow,
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