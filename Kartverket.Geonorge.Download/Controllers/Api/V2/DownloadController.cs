using System;
using System.Configuration;
using System.Net;
using System.Web;
using System.Web.Http;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;
using Kartverket.Geonorge.Download.Services.Auth;

namespace Kartverket.Geonorge.Download.Controllers.Api.V2
{
    public class DownloadController : ApiController
    {
        private readonly IDownloadService _downloadService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IOrderService _orderService;

        public DownloadController
        (
            IOrderService orderService,
            IDownloadService downloadService,
            IAuthenticationService authenticationService
        )
        {
            _orderService = orderService;
            _downloadService = downloadService;
            _authenticationService = authenticationService;
        }

        /// <summary>
        ///     Get file to download for order
        /// </summary>
        /// <param name="orderUuid">The reference number returned from the order</param>
        /// <param name="fileId">The fileId to download from order</param>
        [Route("api/v2/download/order/{orderUuid}/{fileId}")]
        [Route("api/v3/download/order/{orderUuid}/{fileId}")]
        public IHttpActionResult GetFile(string orderUuid, string fileId)
        {
            if (!IsValidUuid(orderUuid))
                return BadRequest("orderUuid is not a valid uuid.");

            if (!IsValidUuid(fileId))
                return BadRequest("fileId is not a valid uuid.");

            var order = _orderService.Find(orderUuid);
            if (order == null)
                return NotFound();

            AuthenticatedUser authenticatedUser = _authenticationService.GetAuthenticatedUser(ControllerContext.Request);
            var userIsLoggedIn = authenticatedUser != null;

            if (order.ContainsRestrictedDatasets() && !userIsLoggedIn)
                return Redirect(UrlToAuthenticationPageWithRedirectToDownloadUrl(orderUuid, fileId));

            if (order.ContainsRestrictedDatasets() && userIsLoggedIn && !order.BelongsToUser(authenticatedUser))
                return Content(HttpStatusCode.Forbidden, "User not allowed to download order");

            var item = order.GetItemWithFileId(fileId);
            if (item == null || !item.IsReadyForDownload())
                return NotFound();

            // Download open data directly from it's location:
            if (item.AccessConstraint.IsOpen())
                return Redirect(item.DownloadUrl);

            // Download restricted data as stream trought this api:
            return Ok(_downloadService.CreateResponseFromRemoteFile(item.DownloadUrl));
        }

        private bool IsValidUuid(string input)
        {
            Guid result;
            return Guid.TryParse(input, out result);
        }

        private string UrlToAuthenticationPageWithRedirectToDownloadUrl(string orderUuid, string fileId)
        {
            var downloadUrl = new DownloadUrlBuilder().OrderId(Guid.Parse(orderUuid)).FileId(Guid.Parse(fileId))
                .Build();
            var encodedReturnUrl = HttpUtility.UrlEncode(downloadUrl);
            var server = ConfigurationManager.AppSettings["DownloadUrl"];
            return $"{server}/AuthServices/SignIn?ReturnUrl={encodedReturnUrl}";
        }
    }
}