using System.Web.Http;

namespace Kartverket.Geonorge.Download.Controllers.Api.V3
{
    public class FileDownloadController : ApiController
    {
        [Route("api/download/file/{datasetUuid}/{fileUuid}")]
        public IHttpActionResult GetFile(string datasetUuid, string fileUuid)
        {
            if (!DownloadV3Controller.IsValidUuid(datasetUuid))
                return BadRequest("datasetUuid is not a valid uuid");

            if (!DownloadV3Controller.IsValidUuid(fileUuid))
                return BadRequest("fileUuid is not a valid uuid");

            return Ok();
        }
    }
}