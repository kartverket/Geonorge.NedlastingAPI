using Asp.Versioning;
using Geonorge.Download.Services.Interfaces;
using Geonorge.NedlastingApi.V3;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Geonorge.Download.Controllers.Api.V4
{
    /// <summary>
    /// Provides endpoints for retrieving dataset capabilities, supported projections, available areas, and formats for the download service API.
    /// Also includes endpoints for validating clipper files and checking if a selected polygon area is within download limits.
    /// All endpoints support both XML and JSON responses.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="config"></param>
    /// <param name="capabilitiesService"></param>
    /// <param name="downloadService"></param>
    /// <param name="webHostEnvironment"></param>
    [ApiController]
    [ApiVersion("4.0")]
    [ApiExplorerSettings(GroupName = "v4")]
    [Route("api/v{version:apiVersion}")]
    [EnableCors("AllowAll")]
    [AllowAnonymous]
    public class CapabilitiesController(
        ILogger<CapabilitiesController> logger, 
        IConfiguration config, 
        ICapabilitiesService capabilitiesService, 
        IDownloadService downloadService, 
        IWebHostEnvironment webHostEnvironment,
        StorageClient storageClient,
        GcsSettings gcsSettings) : ControllerBase
    {
        /// <summary>
        /// Get Capabilities from download service
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        [OutputCache(PolicyName = "MetaTag")]
        [HttpGet("capabilities/{metadataUuid}")]
        [Produces("application/json", "application/xml")]
        [ProducesResponseType(typeof(CapabilitiesType), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetCapabilities([FromRoute] string metadataUuid)
        {
            try
            {
                var capabilities = capabilitiesService.GetCapabilities(Request, metadataUuid);
                if (capabilities == null)
                {
                    logger.LogInformation("Capabilities not found for uuid: " + metadataUuid);
                    return NotFound();
                }
                logger.LogInformation("Capabilities found for uuid: " + metadataUuid);
                Response.Headers.Append("X-API-VERSION", "4.0");
                return Ok(capabilities);
            }
            catch (Exception ex)
            {
                logger.LogError("Error getting capabilities for uuid: " + metadataUuid, ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        /// <summary>
        /// Get Projections from download service
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        [OutputCache(PolicyName = "MetaTag")]
        [HttpGet("codelists/projection/{metadataUuid}")]
        [Produces("application/json", "application/xml")]
        [ProducesResponseType(typeof(List<ProjectionType>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetProjections([FromRoute] string metadataUuid)
        {
            try
            {
                return Ok(capabilitiesService.GetProjections(metadataUuid));
            }
            catch (Exception ex)
            {
                logger.LogError("Error getting projections for uuid: " + metadataUuid, ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get Areas from download service
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        [OutputCache(PolicyName = "MetaTag")]
        [HttpGet("codelists/area/{metadataUuid}")]
        [Produces("application/json", "application/xml")]
        [ProducesResponseType(typeof(List<AreaType>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetAreas([FromRoute] string metadataUuid)
        {
            try
            {
                return Ok(capabilitiesService.GetAllAreas(metadataUuid)); //SJEKK HER. Det surret med kommune og sånn skjer her
            }
            catch (Exception ex)
            {
                logger.LogError("Error getting areas for uuid: " + metadataUuid, ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get Format from download service
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        [OutputCache(PolicyName = "MetaTag")]
        [HttpGet("codelists/format/{metadataUuid}")]
        [Produces("application/json", "application/xml")]
        [ProducesResponseType(typeof(List<FormatType>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetFormats([FromRoute] string metadataUuid)
        {
            try
            {
                return Ok(capabilitiesService.GetFormats(metadataUuid));
            }
            catch (Exception ex)
            {
                logger.LogError("Error getting formats for uuid: " + metadataUuid, ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}