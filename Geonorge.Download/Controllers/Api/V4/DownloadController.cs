using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Geonorge.Download.Services.Interfaces;

namespace Geonorge.Download.Controllers.Api.V4
{
    /// <summary>
    /// For downloading files related to orders. Endpoints to download bundles of files or individual files associated with an order, as well as validate user authentication.
    /// Both open and restricted datasets are handled, ensuring that appropriate access controls are enforced based on user authentication status.
    /// </summary>
    [ApiController]
    [ApiVersion("4.0")]
    [ApiExplorerSettings(GroupName = "v4")]
    [Route("api/v{version:apiVersion}")]
    [EnableCors("AllowKartkatalog")]
    [AllowAnonymous]
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
            {
                return BadRequest("orderUuid is not a valid uuid.");
            }

            var order = orderService.Find(orderUuid);
            if (order == null || string.IsNullOrEmpty(order.DownloadBundleUrl))
            {
                return NotFound();
            }

            if (order.ContainsRestrictedDatasets())
            {
                return Forbid();
            }

            order.DownloadBundleUrl = order.DownloadBundleUrl.Replace("http://", "https://");
            return Redirect(order.DownloadBundleUrl);
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

            if(order.ContainsRestrictedDatasets())
                return Forbid();

            var item = order.GetItemWithFileId(fileId);

            if(item == null || !item.IsReadyForDownload() || item.DownloadUrl == null)
                return NotFound();

            if(item.AccessConstraint.IsRestricted())
                return Forbid();

            var downloadUrl = item.DownloadUrl.Replace("http://", "https://");
                return Redirect(downloadUrl);
            
        }

        internal static bool IsValidUuid(string input)
        {
            Guid result;
            return Guid.TryParse(input, out result);
        }
    }
}
