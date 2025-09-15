using Geonorge.NedlastingApi.V3;
using Geonorge.Download.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Asp.Versioning;
using Geonorge.Download.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Geonorge.Download.Services.Auth;

namespace Geonorge.Download.Controllers.Api.Internal
{
    [ApiController]
    [ApiVersionNeutral]
    [ApiExplorerSettings(GroupName = "internal")]
    [Route("api/internal/download-usage")]
    [EnableCors("AllowKartkatalog")] // Was AllowAllWithCreds
    [Authorize(AuthenticationSchemes = ExternalTokenHandler.SchemeName)]
    //[HandleError]
    public class DownloadUsageController(IOrderService orderService) : ControllerBase
    {

        /// <summary>
        /// Register download usage for statistical purposes. 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Consumes("application/json", "application/xml")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult DownloadUsage([FromBody] DownloadUsageRequestType request)
        {
            orderService.AddOrderUsage(ConvertToDownloadUsage(request));
            return Ok();
        }

        private DownloadUsage ConvertToDownloadUsage(DownloadUsageRequestType request)
        {
            var downloadUsage = new DownloadUsage();
            if (request.entries != null)
            {
                foreach (var entry in request.entries)
                {
                    downloadUsage.AddEntry(new DownloadUsageEntry
                    {
                        Uuid = entry.metadataUuid,
                        AreaCode = entry.areaCode,
                        AreaName = entry.areaName,
                        Format = entry.format,
                        Projection = entry.projection,
                        Group = request.@group,
                        Purpose = request.purpose,
                        SoftwareClient = request.softwareClient,
                        SoftwareClientVersion = request.softwareClientVersion
                    });
                }
            }
            return downloadUsage;
        }
    }
}