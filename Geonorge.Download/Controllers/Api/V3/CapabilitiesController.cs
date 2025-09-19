using Asp.Versioning;
using Geonorge.Download.Models;
using Geonorge.Download.Services.Auth;
using Geonorge.Download.Services.Interfaces;
using Geonorge.NedlastingApi.V3;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;

namespace Geonorge.Download.Controllers.Api.V3
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
    [ApiVersion("3.0")]
    [ApiExplorerSettings(GroupName = "latest")]
    [Route("api")]
    [Route("api/v{version:apiVersion}")]
    [EnableCors("AllowAll")]
    [AllowAnonymous]
    [Authorize(AuthenticationSchemes = $"{BasicMachineAuthHandler.SchemeName},{JwtBearerDefaults.AuthenticationScheme}")]
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
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetAreas([FromRoute] string metadataUuid)
        {
            try
            {
                return Ok(capabilitiesService.GetAreas(metadataUuid, HttpContext.User));
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogError("UnauthorizedAccessException: " + metadataUuid, ex);
                return Unauthorized();
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

        /// <summary>
        /// If polygon is selected, checks if coordinates is within the maximum allowable area that can be downloaded
        /// </summary>
        /// <param name="request">The request containing coordinates, coordinate system, and metadata UUID</param>
        /// <returns></returns>
        [HttpPost("can-download")]
        [Consumes("application/json", "application/xml")]
        [Produces("application/json", "application/xml")]
        [ProducesResponseType(typeof(CanDownloadResponseType), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CanDownload([FromBody] CanDownloadRequestType request)
        {
            try
            {
                var canDownload = true;

                if (config["FmeAreaCheckerEnabled"].Equals("true"))
                    canDownload = downloadService.AreaIsWithinDownloadLimits(request.coordinates,
                        request.coordinateSystem, request.metadataUuid);

                return Ok(new CanDownloadResponseType { canDownload = canDownload });
            }
            catch (Exception ex)
            {
                logger.LogError("Error returning canDownload for uuid: " + request.metadataUuid, ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// If clipper file is selected, checks if file is valid
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        /// <param name="file">The uploaded clipper file</param>
        /// <returns></returns>
        [HttpPost("validate-clipperfile/{metadataUuid}")]
        [Consumes("multipart/form-data")]
        [Produces("application/json", "application/xml")]
        [ProducesResponseType(typeof(ClipperFileResponseType), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidateClipperFile([FromRoute] string metadataUuid, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            if (!CheckFileType(file.FileName))
            {
                return BadRequest("File type not supported.");
            }

            try
            {
                var id = Guid.NewGuid();
                var extension = Path.GetExtension(file.FileName);

                if (extension == ".json")
                    extension = ".geojson";

                var fileName = id + extension;

                var clipperFile = await UploadToGcsAsync(file, storageClient, gcsSettings, HttpContext);

                // Override in local dev
                if (clipperFile.Contains("localhost"))
                    clipperFile = "http://testnedlasting.mypage.download/test.zip";

                var validatorUrl = config["ClipperFileValidator"];
                var token = config["ClipperFileValidatorToken"];

                var resultJson = downloadService.CallClipperFileChecker(
                    $"{validatorUrl}?CLIPPER_FILE={clipperFile}&UUID={metadataUuid}&token={token}");

                logger.LogInformation("Clipper file validation response for uuid: " + metadataUuid + " is: " + resultJson);

                var clipperFileResponseType = new ClipperFileResponseType
                {
                    valid = resultJson.Value<bool>("valid"),
                    message = resultJson.Value<string>("message"),
                    url = clipperFile
                };

                if (!clipperFileResponseType.valid)
                {
                    logger.LogError("Clipper file validation failed for uuid: " + metadataUuid + " with message: " + clipperFileResponseType.message);
                    return StatusCode(StatusCodes.Status500InternalServerError, clipperFileResponseType.message);
                }
                else
                {
                    capabilitiesService.SaveClipperFile(id, clipperFileResponseType.url, clipperFileResponseType.valid, clipperFileResponseType.message);
                    return Created(string.Empty, clipperFileResponseType);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error validating clipper file for UUID: {MetadataUuid}", metadataUuid);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<string> UploadToGcsAsync(
            IFormFile file,
            StorageClient storage,
            GcsSettings gcs,
            HttpContext http)
        {
            var id = Guid.NewGuid().ToString();
            var extension = Path.GetExtension(file.FileName);
            var objectKey = id + extension;

            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".sos"] = "text/vnd.sosi";
            provider.Mappings[".gml"] = "application/gml+xml";
            provider.Mappings[".gdb"] = "application/octet-stream";
            provider.Mappings[".geojson"] = "application/geo+json";
            provider.Mappings[".7z"] = "application/x-7z-compressed";
            if (!provider.TryGetContentType(objectKey, out var contentType))
                contentType = file.ContentType ?? "application/octet-stream";

            await using var src = file.OpenReadStream();
            await storage.UploadObjectAsync(
                bucket: gcs.Bucket,
                objectName: $"clipperfiles/{objectKey}",
                contentType: contentType,
                source: src);

            return $"{http.Request.Scheme}://{http.Request.Host}/clipperfiles/{objectKey}";
        }

        private bool CheckFileType(string fileName)
        {
            string ext = System.IO.Path.GetExtension(fileName);
            switch (ext.ToLower())
            {
                case ".sos":
                    return true;
                case ".zip":
                    return true;
                case ".7z":
                    return true;
                case ".tar":
                    return true;
                case ".gdb":
                    return true;
                case ".gml":
                    return true;
                case ".geojson":
                    return true;
                case ".json":
                    return true;
                case ".gpkg":
                    return true;
                case ".fgb":
                    return true;
                default:
                    return false;
            }
        }
    }
}