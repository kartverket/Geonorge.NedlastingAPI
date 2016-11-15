using System.Web.Http;
using Kartverket.Geonorge.Download.Services;

namespace Kartverket.Geonorge.Download.Controllers.Api.V2
{
    public class DownloadController : ApiController
    {
        private readonly ICapabilitiesService _capabilitiesService;
        private readonly IDownloadService _downloadService;
        private readonly IOrderService _orderService;

        public DownloadController
        (
            IOrderService orderService,
            IDownloadService downloadService,
            ICapabilitiesService capabilitiesService
        )
        {
            _orderService = orderService;
            _downloadService = downloadService;
            _capabilitiesService = capabilitiesService;
        }

        // GET /api/download/order/1307/4047
        [Route("api/download/order/{referenceNumber}/{fileId}")]
        public IHttpActionResult GetFile(int referenceNumber, int fileId)
        {
            var orderReceiptType = _orderService.Find(referenceNumber);
            if (orderReceiptType == null)
                return NotFound();

            // TODO: if (orderReceiptType doesn't belong to authenticated user) return Unauthorized();

            var fileType = _downloadService.GetFileType(orderReceiptType, fileId);
            
            if ((fileType == null) || !_downloadService.IsReadyToDownload(fileType))
                return NotFound();
            
            var dataSet = _capabilitiesService.GetDataset(fileType.metadataUuid);
            if (dataSet == null)
                return NotFound();

            // Download open data directly from it's location:
            if (string.IsNullOrEmpty(dataSet.AccessConstraint))
                return Redirect(fileType.downloadUrl);
            
            // Download restricted data as stream trought this api:
            return Ok(_downloadService.CreateResponseFromRemoteFile(fileType.downloadUrl));
        }
    }
}
