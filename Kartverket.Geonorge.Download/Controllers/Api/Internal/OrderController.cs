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
    [RequireHttpsNonLocal]
    [System.Web.Http.Authorize(Roles = AuthConfig.DatasetProviderRole)]
    public class ManageOrderController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IOrderService _orderService;

        public ManageOrderController(IOrderService orderService)
        {
            _orderService = orderService;
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
                    
                    DownloadUrl = request.DownloadUrl,
                    Message = request.Message
                };
                
                int fileIdAsInt;
                if (!int.TryParse(request.FileId, out fileIdAsInt))
                {
                    Log.Info("Bad request - invalid file id: " + request.FileId);
                    return BadRequest("Invalid file id.");
                }
                updateFileStatusInformation.FileId = fileIdAsInt;

                OrderItemStatus itemStatus;
                if (!System.Enum.TryParse(request.Status, true, out itemStatus))
                {
                    Log.Info("Bad request - invalid file status: " + request.Status);
                    return BadRequest("Invalid file status, valid values are: [WaitingForProcessing, ReadyForDownload, Error]");
                }
                updateFileStatusInformation.Status = itemStatus;


                _orderService.UpdateFileStatus(updateFileStatusInformation);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
            return Ok();
        }
    }
}