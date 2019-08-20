using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;
using Kartverket.Geonorge.Download.Services.Auth;
using log4net;

namespace Kartverket.Geonorge.Download.Controllers.Api.V3
{
    public class FileDownloadController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IFileService _fileService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IDownloadService _downloadService;

        public FileDownloadController(IFileService fileService, IAuthenticationService authenticationService, IDownloadService downloadService)
        {
            _fileService = fileService;
            _authenticationService = authenticationService;
            _downloadService = downloadService;
        }

        /// <summary>
        /// Download a file directly based on dataset uuid and file uuid. This method is used by the atom feed and desktop download client.
        /// Restricted files are secured with BAAT-authentication (SAML) and local machine accounts (Basic authentication)
        /// </summary>
        /// <param name="datasetUuid">metadata uuid of the dataset</param>
        /// <param name="fileUuid">the file uuid</param>
        /// <returns></returns>
        [System.Web.Http.Route("api/download/file/{datasetUuid}/{fileUuid}")]
        public async Task<IHttpActionResult> GetFile(string datasetUuid, string fileUuid)
        {
            if (!DownloadV3Controller.IsValidUuid(datasetUuid))
                return BadRequest("datasetUuid is not a valid uuid");

            if (!DownloadV3Controller.IsValidUuid(fileUuid))
                return BadRequest("fileUuid is not a valid uuid");

            Dataset dataset = await _fileService.GetDatasetAsync(datasetUuid);
            if (dataset == null)
                return NotFound();

            File file = await _fileService.GetFileAsync(fileUuid, datasetUuid);
            if (file == null)
                return NotFound();

            var userIsLoggedIn = UserIsLoggedIn();
            var isRestrictedDataset = dataset.IsRestricted();
            if (isRestrictedDataset && !userIsLoggedIn)
            {
                Log.Info($"Access denied to [file={file.Filename}]. [dataset={dataset.Title}] is restricted and user must be logged in.");

                if (Request.Headers.Accept.First().MediaType.Equals("text/html")) // be kind to browsers and redirect to login page
                    return Redirect(DownloadV3Controller.UrlToAuthenticationPageWithRedirectToDownloadUrl("/api/download/file/" + datasetUuid + "/" + fileUuid));

                return StatusCode(HttpStatusCode.Forbidden);
            }

            Log.Info($"Serving [file={file.Filename}] [dataset={dataset.Title}] [datasetUuid={dataset.MetadataUuid}] [isRestrictedDataset={isRestrictedDataset}]");

            return Ok(_downloadService.CreateResponseFromRemoteFile(file.Url));
        }

        private bool UserIsLoggedIn()
        {
            if(ClaimsPrincipal.Current != null && ClaimsPrincipal.Current.Identity.IsAuthenticated)
                return true;

            AuthenticatedUser authenticatedUser = _authenticationService.GetAuthenticatedUser(ControllerContext.Request);
            return authenticatedUser != null;
        }
    }
}