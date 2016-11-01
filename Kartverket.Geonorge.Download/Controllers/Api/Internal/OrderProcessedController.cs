using System.Reflection;
using System.Web.Http;
using Kartverket.Geonorge.Download.Models.Api.Internal;
using log4net;

namespace Kartverket.Geonorge.Download.Controllers.Api.Internal
{
    [RoutePrefix("api/internal/order")]
    public class OrderProcessedController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Update status on a given file that has been processed.
        /// </summary>
        /// <param name="request">Updated information about a file.</param>
        /// <returns>HTTP status codes 200 if ok.</returns>
        [Route("update-file-status")]
        [HttpPost]
        public IHttpActionResult UpdateFileStatus(UpdateFileStatusRequest request)
        {
            Log.Warn("UpdateFileStatus is not implemented.");

            return Ok();
        }
    }
}