using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using Geonorge.NedlastingApi.V2;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;
using log4net;

namespace Kartverket.Geonorge.Download.Controllers.Api
{
    public class CapabilitiesGlobalController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ICapabilitiesService _capabilitiesService;

        public CapabilitiesGlobalController(ICapabilitiesService capabilitiesService)
        {
            _capabilitiesService = capabilitiesService;
        }

        [HttpGet]
        [Route("api")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IHttpActionResult Index()
        {
            return Ok(ApiDescription());
        }

        [HttpGet]
        [Route("api/capabilities")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IHttpActionResult CapabilitiesIndex()
        {
            if (HtmlIsFirstElementOfAcceptHeader())
                return Redirect(UrlToHelpPages());

            return Ok(ApiDescription());
        }

        private string UrlToHelpPages()
        {
            return Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Help/Api/GET-api-capabilities-metadataUuid";
        }

        private bool HtmlIsFirstElementOfAcceptHeader()
        {
            return Request.Headers.Accept.First().MediaType.Equals("text/html");
        }

        private static VersionResponseType ApiDescription()
        {
            var capabilityLinksCreator = new CapabilityLinksCreator();
            var apiVersionDescription = new VersionResponseType
            {
                version = capabilityLinksCreator.GetDefaultApiVersion(),
                _links = capabilityLinksCreator.CreateLinks().ToArray()
            };
            return apiVersionDescription;
        }


        /// <summary>
        ///     Get Capabilities from download service
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        [Route("api/capabilities/{metadataUuid}")]
        [ResponseType(typeof(CapabilitiesType))]
        public IHttpActionResult GetCapabilities(string metadataUuid)
        {
            try
            {
                var capabilities = _capabilitiesService.GetCapabilities(metadataUuid);
                if (capabilities == null)
                {
                    Log.Info("Capabilities not found for uuid: " + metadataUuid);
                    return NotFound();
                }
                Log.Info("Capabilities found for uuid: " + metadataUuid);
                return Ok(capabilities);
            }
            catch (Exception ex)
            {
                Log.Error("Error getting capabilities for uuid: " + metadataUuid, ex);
                return InternalServerError();
            }
        }
    }
}