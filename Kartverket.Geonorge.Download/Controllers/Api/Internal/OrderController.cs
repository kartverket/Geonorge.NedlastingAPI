using System;
using System.Reflection;
using System.Web.Http;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Models.Api.Internal;
using Kartverket.Geonorge.Download.Services;
using log4net;

namespace Kartverket.Geonorge.Download.Controllers.Api.Internal
{
    [System.Web.Http.RoutePrefix("api/internal/order")]
    [System.Web.Http.Authorize(Roles = AuthConfig.DatasetProviderRole)]
    public class ManageOrderController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IOrderService _orderService;
        private readonly INotificationService _notificationService;

        public ManageOrderController(IOrderService orderService, INotificationService notificationService)
        {
            _orderService = orderService;
            _notificationService = notificationService;
        }

        /// <summary>
        ///     Update status on a given file that has been processed.
        /// </summary>
        /// <param name="request">Updated information about a file.</param>
        /// <returns>HTTP status codes 200 if ok.</returns>
        [System.Web.Http.Route("update-file-status")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult UpdateFileStatus(UpdateFileStatusRequest request)
        {
            try
            {
                UpdateFileStatusInformation updateFileStatusInformation = new UpdateFileStatusInformation
                {
                    FileId = request.FileId,
                    DownloadUrl = request.DownloadUrl,
                    Message = request.Message
                };
                
                OrderItemStatus itemStatus;
                if (!System.Enum.TryParse(request.Status, true, out itemStatus))
                {
                    Log.Info("Bad request - invalid file status: " + request.Status);
                    return BadRequest("Invalid file status, valid values are: [WaitingForProcessing, ReadyForDownload, Error]");
                }
                updateFileStatusInformation.Status = itemStatus;


                _orderService.UpdateFileStatus(updateFileStatusInformation);
                
                if (request.Status == "ReadyForDownload")
                    _notificationService.SendReadyForDownloadNotification(request.FileId);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
            return Ok();
        }
    }
}