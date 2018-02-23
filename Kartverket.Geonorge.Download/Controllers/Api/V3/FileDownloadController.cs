using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
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

        [Route("api/download/file/{datasetUuid}/{fileUuid}")]
        public async Task<IHttpActionResult> GetFile(string datasetUuid, string fileUuid)
        {
            if (!DownloadV3Controller.IsValidUuid(datasetUuid))
                return BadRequest("datasetUuid is not a valid uuid");

            if (!DownloadV3Controller.IsValidUuid(fileUuid))
                return BadRequest("fileUuid is not a valid uuid");

            Dataset dataset = await _fileService.GetDatasetAsync(datasetUuid);
            if (dataset == null)
                return NotFound();

            File file = await _fileService.GetFileAsync(fileUuid);
            if (file == null)
                return NotFound();

            var userIsLoggedIn = UserIsLoggedIn();
            var isRestrictedDataset = dataset.IsRestricted();
            if (isRestrictedDataset && !userIsLoggedIn)
            {
                Log.Info($"Access denied to [file={file.Filename}], dataset is restricted and user must be logged in.");
                return StatusCode(HttpStatusCode.Forbidden);
            }

            Log.Info($"Serving [file={file.Filename}] [dataset={dataset.Title}] [datasetUuid={dataset.MetadataUuid}] [isRestrictedDataset={isRestrictedDataset}]");

            return Ok(_downloadService.CreateResponseFromRemoteFile(file.Url));
        }

        private bool UserIsLoggedIn()
        {
            AuthenticatedUser authenticatedUser = _authenticationService.GetAuthenticatedUser(ControllerContext.Request);
            return authenticatedUser != null;
        }
    }
}