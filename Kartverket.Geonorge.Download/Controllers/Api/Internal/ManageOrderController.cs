using System;
using System.Reflection;
using System.Web.Http;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Models.Api.Internal;
using Kartverket.Geonorge.Download.Services;
using log4net;

namespace Kartverket.Geonorge.Download.Controllers.Api.Internal
{
    [RoutePrefix("api/internal/order")]
    [Authorize(Roles = AuthConfig.DatasetProviderRole)]
    public class ManageOrderController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IUpdateFileStatusService _updateFileStatusService;

        public ManageOrderController(IUpdateFileStatusService updateFileStatusService)
        {
            _updateFileStatusService = updateFileStatusService;
        }

        /// <summary>
        ///     Update status on a given file that has been processed.
        /// </summary>
        /// <param name="request">Updated information about a file.</param>
        /// <returns>HTTP status codes 200 if ok.</returns>
        [Route("update-file-status")]
        [HttpPost]
        public IHttpActionResult UpdateFileStatus(UpdateFileStatusRequest request)
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
                    Log.Info("Bad request - invalid file status: " + request.Status);
                    return BadRequest(
                        "Invalid file status, valid values are: [WaitingForProcessing, ReadyForDownload, Error]");
                }
                updateFileStatusInformation.Status = itemStatus;

                _updateFileStatusService.UpdateFileStatus(updateFileStatusInformation);
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
                return InternalServerError(e);
            }
            return Ok();
        }

        [Route("update-order-status")]
        [HttpPost]
        public IHttpActionResult UpdateOrderStatus(UpdateOrderStatusRequest request)
        {
            try
            {
                Log.Info($"UpdateOrderStatus invoked for order: {request.OrderUuid} - not implemented yet");
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
                return InternalServerError(e);
            }
            return Ok();
        }
    }
}