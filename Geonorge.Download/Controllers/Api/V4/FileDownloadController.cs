using Asp.Versioning;
using Geonorge.Download.Controllers.Api;
using Geonorge.Download.Models;
using Geonorge.Download.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Geonorge.Download.Controllers.Api.V4
{
    [ApiController]
    [ApiVersion("4.0")]
    [ApiVersionHeader("4.0")]
    [Tags("Download")] // Groups together with DownloadControllers endpoints
    [ApiExplorerSettings(GroupName = "v4")]
    [Route("api/v{version:apiVersion}")]
    [AllowAnonymous]
    public class FileDownloadController(ILogger<FileDownloadController> logger, IConfiguration config, IFileService fileService, IDownloadService downloadService) : ControllerBase
    {
        /// <summary>
        /// Download a file directly based on dataset uuid and file uuid. This method is used by the atom feed and desktop download client.
        /// Restricted files are secured with BAAT-authentication (SAML) and local machine accounts (Basic authentication)
        /// </summary>
        /// <param name="datasetUuid">metadata uuid of the dataset</param>
        /// <param name="fileUuid">the file uuid</param>
        /// <returns></returns>
        [HttpGet("download/file/{datasetUuid}/{fileUuid}")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK, "application/octet-stream")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/json", "application/xml")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden, "application/json", "application/xml")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/json", "application/xml")]
        public async Task<IActionResult> GetFile(string datasetUuid, string fileUuid)
        {
            if (!DownloadController.IsValidUuid(datasetUuid))
                return BadRequest("datasetUuid is not a valid uuid");

            if (!DownloadController.IsValidUuid(fileUuid))
                return BadRequest("fileUuid is not a valid uuid");

            Dataset dataset = await fileService.GetDatasetAsync(datasetUuid);
            if (dataset == null)
                return NotFound();

            if(dataset.IsRestricted()) 
                return Forbid();

            Models.File file = await fileService.GetFileAsync(fileUuid, datasetUuid);
            if (file == null)
                return NotFound();            

            logger.LogInformation($"Serving [file={file.Filename}] [dataset={dataset.Title}] [datasetUuid={dataset.MetadataUuid}] [isRestrictedDataset={false}]");

            await downloadService.StreamRemoteFileToResponseAsync(HttpContext, file.Url);

            return new EmptyResult(); 
        }
    }
}