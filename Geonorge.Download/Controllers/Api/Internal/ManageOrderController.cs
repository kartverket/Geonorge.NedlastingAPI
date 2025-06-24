using Geonorge.Download.Models;
using Geonorge.Download.Models.Api.Internal;
using Geonorge.Download.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Geonorge.Download.Controllers.Api.Internal
{
    [Route("api/internal/order")]
    [Authorize(Roles = AuthConfig.DatasetProviderRole)]
    public class ManageOrderController(ILogger<ManageOrderController> logger, IUpdateFileStatusService updateFileStatusService, IOrderService orderService) : ControllerBase
    {
        /// <summary>
        ///     Update status on a given file that has been processed.
        /// </summary>
        /// <param name="request">Updated information about a file.</param>
        /// <returns>HTTP status codes 200 if ok.</returns>
        [Route("update-file-status")]
        [HttpPost]
        public IActionResult UpdateFileStatus(UpdateFileStatusRequest request)
        {
            try
            {
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

                updateFileStatusService.UpdateFileStatus(updateFileStatusInformation);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message, e);
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
            return Ok();
        }

        [Route("update-order-status")]
        [HttpPost]
        public IActionResult UpdateOrderStatus(UpdateOrderStatusRequest orderStatus)
        {
            try
            {
                logger.LogInformation($"UpdateOrderStatus invoked for order: {orderStatus.OrderUuid}");

                orderService.UpdateOrderStatus(orderStatus);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message, e);
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
            return Ok();
        }


        /// <summary>
        ///     Inform user about file clipping status
        /// </summary>
        /// <returns>HTTP status codes 200 if ok.</returns>
        [Route("status-notification")]
        [HttpGet]
        public IActionResult StatusNotification()
        {
            try
            {
                logger.LogInformation($"StatusNotification invoked");

                orderService.SendStatusNotification();
            }
            catch (Exception e)
            {
                logger.LogError(e.Message, e);
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
            return Ok();
        }

        /// <summary>
        ///     Inform user about file clipping jobs that will not be delivered
        /// </summary>
        /// <returns>HTTP status codes 200 if ok.</returns>
        [Route("status-notification-not-deliverable")]
        [HttpGet]
        public IActionResult StatusNotificationNotDeliverable()
        {
            try
            {
                logger.LogInformation($"StatusNotificationNotDeliverable invoked");
                orderService.SendStatusNotificationNotDeliverable();
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