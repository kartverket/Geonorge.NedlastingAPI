using System;
using System.Configuration;
using System.Net;
using System.Web;
using Geonorge.Download.Models;
using Geonorge.Download.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.CodeAnalysis;
using Geonorge.Download.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Geonorge.Download.Controllers.Api.V3
{
    /// <summary>
    /// For downloading files related to orders. Endpoints to download bundles of files or individual files associated with an order, as well as validate user authentication.
    /// Both open and restricted datasets are handled, ensuring that appropriate access controls are enforced based on user authentication status.
    /// </summary>
    [ApiController]
    [ApiVersion("3.0")]
    [ApiExplorerSettings(GroupName = "latest")]
    [Route("api")]
    [Route("api/v{version:apiVersion}")]
    [EnableCors("AllowKartkatalog")]
    [AllowAnonymous]
    [Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme},{BasicMachineAuthHandler.SchemeName}")]
    public class DownloadController(IConfiguration config, IDownloadService downloadService, IOrderService orderService) : ControllerBase
    {

        /// <summary>
        /// Get file to download for order
        /// </summary>
        /// <param name="orderUuid">The reference number returned from the order</param>
        [HttpGet("download/order/{orderUuid}/bundle")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK, "application/octet-stream")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/json", "application/xml")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden, "application/json", "application/xml")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/json", "application/xml")]
        public async Task<IActionResult> GetBundleFile([FromRoute] string orderUuid)
        {
            if (!IsValidUuid(orderUuid))
                return BadRequest("orderUuid is not a valid uuid.");

            var order = orderService.Find(orderUuid);
            if (order == null)
                return NotFound();

            bool userIsLoggedIn = (HttpContext.User.Identity != null) ? HttpContext.User.Identity.IsAuthenticated : false;

            if (order.ContainsRestrictedDatasets() && !userIsLoggedIn)
            {
                var downloadUrl = new DownloadUrlBuilder(config)
                    .OrderId(Guid.Parse(orderUuid))
                    .AsBundle();
                return Redirect(UrlToAuthenticationPageWithRedirectToDownloadUrl(downloadUrl));
            }

            if (order.ContainsRestrictedDatasets() && userIsLoggedIn && !order.BelongsToUser(HttpContext.User))
                return Forbid(); // "User not allowed to download order"

            if (string.IsNullOrEmpty(order.DownloadBundleUrl))
                return NotFound();

            // Download open data directly from it's location:
            if (!order.ContainsRestrictedDatasets())
            {
                order.DownloadBundleUrl = order.DownloadBundleUrl.Replace("http://", "https://");
                return Redirect(order.DownloadBundleUrl); 
            }

            // Stream restricted data directly to the client
            await downloadService.StreamRemoteFileToResponseAsync(HttpContext, order.DownloadBundleUrl);

            // Response already written
            return NoContent();

            // Download restricted data as stream trought this api:
            //return Ok(downloadService.CreateResponseFromRemoteFile(order.DownloadBundleUrl));
        }

        /// <summary>
        /// Get file to download for order
        /// </summary>
        /// <param name="orderUuid">The reference number returned from the order</param>
        /// <param name="fileId">The fileId to download from order</param>
        [HttpGet("download/order/{orderUuid}/{fileId}")]
        //[Produces("application/octet-stream")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK, "application/octet-stream")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/json", "application/xml")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden, "application/json", "application/xml")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/json", "application/xml")]
        public async Task<IActionResult> GetFile([FromRoute] string orderUuid, [FromRoute] string fileId)
        {
            if (!IsValidUuid(orderUuid))
                return BadRequest("orderUuid is not a valid uuid.");

            if (!IsValidUuid(fileId))
                return BadRequest("fileId is not a valid uuid.");

            var order = orderService.Find(orderUuid);
            if (order == null)
                return NotFound();

            bool userIsLoggedIn = (HttpContext.User.Identity != null) ? HttpContext.User.Identity.IsAuthenticated : false;

            if (order.ContainsRestrictedDatasets() && !userIsLoggedIn)
            {
                var downloadUrl = new DownloadUrlBuilder(config)
                    .OrderId(Guid.Parse(orderUuid))
                    .FileId(Guid.Parse(fileId))
                    .Build();

                return Redirect(UrlToAuthenticationPageWithRedirectToDownloadUrl(downloadUrl));
            }

            if (order.ContainsRestrictedDatasets() && userIsLoggedIn && !order.BelongsToUser(HttpContext.User))
                return Forbid(); // "User not allowed to download order"

            var item = order.GetItemWithFileId(fileId);
            if (item == null || !item.IsReadyForDownload())
                return NotFound();

            // Download open data directly from it's location:
            if (item.AccessConstraint.IsOpen())
            {
                item.DownloadUrl = item.DownloadUrl.Replace("http://", "https://");
                return Redirect(item.DownloadUrl); 
            }

            // Stream restricted data directly to the client
            await downloadService.StreamRemoteFileToResponseAsync(HttpContext, item.DownloadUrl);

            // Response already written
            return NoContent();

            // Download restricted data as stream trought this api:
            //return Ok(downloadService.CreateResponseFromRemoteFile(item.DownloadUrl));
        }

        /// <summary>
        /// Validate user
        /// </summary>
        [HttpGet("download/validate-user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ValidateUser()
        {
            bool userIsLoggedIn = (HttpContext.User.Identity != null) ? HttpContext.User.Identity.IsAuthenticated : false;
            if (!userIsLoggedIn)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Feil brukernavn/passord");
            }
            return Ok();
        }

        internal static bool IsValidUuid(string input)
        {
            Guid result;
            return Guid.TryParse(input, out result);
        }

        private string UrlToAuthenticationPageWithRedirectToDownloadUrl(string downloadUrl)
        {
            var server = config["DownloadUrl"];
            var encodedReturnUrl = HttpUtility.UrlEncode(downloadUrl);
            return $"{server}/Home/SignIn?ReturnUrl={encodedReturnUrl}";
        }
    }
}