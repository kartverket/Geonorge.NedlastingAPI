using System.Configuration;
using System.Web.Http;
using Geonorge.NedlastingApi.V2;

namespace Kartverket.Geonorge.Download.Controllers.Api
{
    public class VersionController : ApiController
    {
        [HttpGet]
        [Route("api/version")]
        public IHttpActionResult Index()
        {
            var serverUrl = ConfigurationManager.AppSettings["DownloadUrl"];
            return Ok(new VersionResponseType
            {
                version = "2",
                _links = new[]
                {
                    new LinkType {rel = "capabilities", href = serverUrl + "/api/v2/capabilities/{metadataUuid}", templated = true},
                    new LinkType {rel = "create-order", href = serverUrl + "/api/v2/order"},
                    new LinkType {rel = "get-order", href = serverUrl + "/api/v2/order"},
                    new LinkType {rel = "download-file", href = serverUrl + "/api/v2/download/order/{referenceNumber}/{fileId}", templated = true},
                }
            });
        }
    }
}