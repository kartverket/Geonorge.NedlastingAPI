using Asp.Versioning;
using Geonorge.Download.Models;
using Geonorge.Download.Services;
using Geonorge.Download.Services.Auth;
using Geonorge.Download.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace Geonorge.Download.Controllers.Api.V3
{
    [ApiController]
    [ApiVersion("3.0")]
    [Tags("Download")] // Groups together with DownloadControllers endpoints
    [ApiExplorerSettings(GroupName = "latest")]
    [Route("api")]
    [Route("api/v{version:apiVersion}")]
    [AllowAnonymous]
    [Authorize(AuthenticationSchemes = $"{BasicMachineAuthHandler.SchemeName},{JwtBearerDefaults.AuthenticationScheme}")]
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

            Models.File file = await fileService.GetFileAsync(fileUuid, datasetUuid);
            if (file == null)
                return NotFound();


            var user = HttpContext.User.Identity;
            bool userIsLoggedIn = (HttpContext.User.Identity != null) ? HttpContext.User.Identity.IsAuthenticated : false;
            var isRestrictedDataset = dataset.IsRestricted();
            if (isRestrictedDataset && !userIsLoggedIn)
            {
                logger.LogInformation($"Access denied to [file={file.Filename}]. [dataset={dataset.Title}] is restricted and user must be logged in.");


                if (Request.Headers.Accept.Any(h => h.Contains("text/html"))) // be kind to browsers and redirect to login page
                    return Redirect(UrlToAuthenticationPageWithRedirectToDownloadUrl(config["DownloadUrl"] + "/api/download/file/" + datasetUuid + "/" + fileUuid));

                return Forbid(BasicMachineAuthHandler.SchemeName); // TODO: Make smart scheme?
            }

            if (isRestrictedDataset && userIsLoggedIn)
            {
                var userHasAccess = fileService.HasAccess(file, HttpContext.User);
                if (!userHasAccess)
                {
                    logger.LogInformation($"Access denied to [file={file.Filename}]. [dataset={dataset.Title}] is restricted and user does not have required access/role.");
                    return Forbid(BasicMachineAuthHandler.SchemeName); // TODO: Make smart scheme?
                }
            }

            logger.LogInformation($"Serving [file={file.Filename}] [dataset={dataset.Title}] [datasetUuid={dataset.MetadataUuid}] [isRestrictedDataset={isRestrictedDataset}]");

            await downloadService.StreamRemoteFileToResponseAsync(HttpContext, file.Url);

            return NoContent();
        }

        private string UrlToAuthenticationPageWithRedirectToDownloadUrl(string downloadUrl)
        {
            var server = config["DownloadUrl"];
            var encodedReturnUrl = HttpUtility.UrlEncode(downloadUrl);
            return $"{server}account/login?ReturnUrl={encodedReturnUrl}";
        }
    }
}