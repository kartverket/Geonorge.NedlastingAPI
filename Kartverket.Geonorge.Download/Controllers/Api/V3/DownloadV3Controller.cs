using System;
using System.Configuration;
using System.Net;
using System.Web;
using System.Web.Http;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;
using Kartverket.Geonorge.Download.Services.Auth;

namespace Kartverket.Geonorge.Download.Controllers.Api.V3
{
    public class DownloadV3Controller : ApiController
    {
        private readonly IDownloadService _downloadService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IOrderService _orderService;

        public DownloadV3Controller
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
        [Route("api/download/order/{orderUuid}/bundle")]
        public IHttpActionResult GetBundleFile(string orderUuid)
        {
            if (!IsValidUuid(orderUuid))
                return BadRequest("orderUuid is not a valid uuid.");

            var order = _orderService.Find(orderUuid);
            if (order == null)
                return NotFound();

            AuthenticatedUser authenticatedUser = GetAuthenticatedUser();            
            var userIsLoggedIn = authenticatedUser != null;

            if (order.ContainsRestrictedDatasets() && !userIsLoggedIn)
            {
                var downloadUrl = new DownloadUrlBuilder()
                    .OrderId(Guid.Parse(orderUuid))
                    .AsBundle();
                return Redirect(UrlToAuthenticationPageWithRedirectToDownloadUrl(downloadUrl));
            }

            if (order.ContainsRestrictedDatasets() && userIsLoggedIn && !order.BelongsToUser(authenticatedUser))
                return Content(HttpStatusCode.Forbidden, "User not allowed to download order");
            
            if (string.IsNullOrEmpty(order.DownloadBundleUrl))
                return NotFound();

            // Download open data directly from it's location:
            if (!order.ContainsRestrictedDatasets())
                return Redirect(order.DownloadBundleUrl);

            // Download restricted data as stream trought this api:
            return Ok(_downloadService.CreateResponseFromRemoteFile(order.DownloadBundleUrl));
        }

        /// <summary>
        ///     Get file to download for order
        /// </summary>
        /// <param name="orderUuid">The reference number returned from the order</param>
        /// <param name="fileId">The fileId to download from order</param>
        [Route("api/download/order/{orderUuid}/{fileId}")]
        public IHttpActionResult GetFile(string orderUuid, string fileId)
        {
            if (!IsValidUuid(orderUuid))
                return BadRequest("orderUuid is not a valid uuid.");

            if (!IsValidUuid(fileId))
                return BadRequest("fileId is not a valid uuid.");

            var order = _orderService.Find(orderUuid);
            if (order == null)
                return NotFound();

            AuthenticatedUser authenticatedUser = GetAuthenticatedUser();

            var userIsLoggedIn = authenticatedUser != null;

            if (order.ContainsRestrictedDatasets() && !userIsLoggedIn)
            {
                var downloadUrl = new DownloadUrlBuilder()
                    .OrderId(Guid.Parse(orderUuid))
                    .FileId(Guid.Parse(fileId))
                    .Build();
                return Redirect(UrlToAuthenticationPageWithRedirectToDownloadUrl(downloadUrl));
            }

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

        private AuthenticatedUser GetAuthenticatedUser()
        {
            return _authenticationService.GetAuthenticatedUser(ControllerContext.Request);
        }

        private bool IsValidUuid(string input)
        {
            Guid result;
            return Guid.TryParse(input, out result);
        }

        private string UrlToAuthenticationPageWithRedirectToDownloadUrl(string downloadUrl)
        {
            var encodedReturnUrl = HttpUtility.UrlEncode(downloadUrl);
            var server = ConfigurationManager.AppSettings["DownloadUrl"];
            return $"{server}/AuthServices/SignIn?ReturnUrl={encodedReturnUrl}";
        }
    }
}