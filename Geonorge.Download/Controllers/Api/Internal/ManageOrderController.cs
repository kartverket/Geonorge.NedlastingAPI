using Asp.Versioning;
using Geonorge.Download.Models;
using Geonorge.Download.Models.Api.Internal;
using Geonorge.Download.Services.Auth;
using Geonorge.Download.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Geonorge.Download.Controllers.Api.Internal
{
    [ApiController]
    [ApiVersionNeutral]
    [ApiExplorerSettings(GroupName = "internal")]
    [Route("api/internal/order")]
    [Authorize(AuthenticationSchemes = BasicAuthHandler.SchemeName, Roles = AuthConfig.DatasetProviderRole)]
    public class ManageOrderController(ILogger<ManageOrderController> logger, IUpdateFileStatusService updateFileStatusService, IOrderService orderService) : ControllerBase
    {
        /// <summary>
        /// Update status on a given file that has been processed.
        /// </summary>
        /// <returns>HTTP status codes 200 if ok.</returns>
        [HttpPost("update-file-status")]
        public async Task<IActionResult> UpdateFileStatus()
        {
            // TODO: FME Should use headers for Content-Type=application/json instead of adding it in query parameter.
            try
            {
                UpdateFileStatusRequest? request;

                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    request = JsonSerializer.Deserialize<UpdateFileStatusRequest>(
                        body,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                }

                if (request == null)
                {
                    logger.LogInformation("Bad request - could not deserialize request body.");
                    return BadRequest("Invalid request body.");
                }

                var updateFileStatusInformation = new UpdateFileStatusInformation
                {
                    FileId = request.FileId,
                    DownloadUrl = request.DownloadUrl,
                    Message = request.Message
                };

                OrderItemStatus itemStatus;
                if (!Enum.TryParse(request.Status, true, out itemStatus))
                {
                    logger.LogInformation("Bad request - invalid file status: " + request.Status);
                    return BadRequest(
                        "Invalid file status, valid values are: [WaitingForProcessing, ReadyForDownload, Error]");
                }
                updateFileStatusInformation.Status = itemStatus;

                await updateFileStatusService.UpdateFileStatus(updateFileStatusInformation);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to update file status");
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
            return Ok();
        }

        [HttpPost("update-order-status")]
        public async Task<IActionResult> UpdateOrderStatus()
        {
            // TODO: FME Should use headers for Content-Type=application/json instead of adding it in query parameter.
            try
            {
                UpdateOrderStatusRequest? orderStatus;

                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    orderStatus = JsonSerializer.Deserialize<UpdateOrderStatusRequest>(
                        body,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                }

                if (orderStatus == null)
                {
                    logger.LogInformation("Bad request - could not deserialize request body.");
                    return BadRequest("Invalid request body.");
                }

                logger.LogInformation("UpdateOrderStatus invoked for order: {OrderUuid}", orderStatus.OrderUuid);

                await orderService.UpdateOrderStatus(orderStatus);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to update order status");
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }

            return Ok();
        }


        /// <summary>
        /// Inform user about file clipping status
        /// </summary>
        /// <returns>HTTP status codes 200 if ok.</returns>
        [HttpGet("status-notification")]
        public async Task<IActionResult> StatusNotification()
        {
            try
            {
                logger.LogInformation($"StatusNotification invoked");

                await orderService.SendStatusNotification();
            }
            catch (Exception e)
            {
                logger.LogError(e.Message, e);
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
            return Ok();
        }

        /// <summary>
        /// Inform user about file clipping jobs that will not be delivered
        /// </summary>
        /// <returns>HTTP status codes 200 if ok.</returns>
        [HttpGet("status-notification-not-deliverable")]
        public async Task<IActionResult> StatusNotificationNotDeliverable()
        {
            try
            {
                logger.LogInformation($"StatusNotificationNotDeliverable invoked");
                await orderService.SendStatusNotificationNotDeliverable();
            }
            catch (Exception e)
            {
                logger.LogError(e.Message, e);
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
            return Ok();
        }


    }
}