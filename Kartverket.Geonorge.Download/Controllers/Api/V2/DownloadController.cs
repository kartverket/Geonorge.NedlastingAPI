using System;
using System.Configuration;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;
using Kartverket.Geonorge.Utilities;

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
        
        [Route("api/v2/download/order/{orderUuid}/{fileId}")]
        public IHttpActionResult GetFile(string orderUuid, string fileId)
        {
            Order order = _orderService.Find(orderUuid);
            if (order == null)
                return NotFound();

            string username = SecurityClaim.GetUsername();

            if (!order.CanBeDownloadedByUser(username))
                return Redirect(UrlToAuthenticationPageWithRedirectToDownloadUrl(orderUuid, fileId));

            OrderItem item = order.GetItemWithFileId(fileId);
            if (item == null || !item.IsReadyForDownload())
                return NotFound();
            
            // Download open data directly from it's location:
            if (item.AccessConstraint.IsOpen())
                return Redirect(item.DownloadUrl);
            
            // Download restricted data as stream trought this api:
            return Ok(_downloadService.CreateResponseFromRemoteFile(item.DownloadUrl));
        }

        private string UrlToAuthenticationPageWithRedirectToDownloadUrl(string orderUuid, string fileId)
        {
            var downloadUrl = new DownloadUrlBuilder().OrderId(Guid.Parse(orderUuid)).FileId(Guid.Parse(fileId)).Build();
            var encodedReturnUrl = HttpUtility.UrlEncode(downloadUrl);
            string server = ConfigurationManager.AppSettings["DownloadUrl"];
            return $"{server}/AuthServices/SignIn?ReturnUrl={encodedReturnUrl}";
        }
    }
}
