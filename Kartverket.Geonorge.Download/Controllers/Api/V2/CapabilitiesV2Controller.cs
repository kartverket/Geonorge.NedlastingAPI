using System;
using System.Configuration;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Web.Mvc;
using Geonorge.NedlastingApi.V2;
using Kartverket.Geonorge.Download.Services;
using log4net;

namespace Kartverket.Geonorge.Download.Controllers.Api.V2
{
    [HandleError]
    [EnableCors("*", "*", "*")]
    [System.Web.Http.RoutePrefix("api/v2")]
    public class CapabilitiesV2Controller : ApiController
    {
        private readonly ICapabilitiesService _capabilitiesService;
        private readonly IDownloadService _downloadService;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public CapabilitiesV2Controller(ICapabilitiesService capabilitiesService, IDownloadService downloadService)
        {
            _capabilitiesService = capabilitiesService;
            _downloadService = downloadService;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("capabilities")]
        public IHttpActionResult Index()
        {
            var uri = new Uri(WebConfigurationManager.AppSettings["DownloadUrl"] + "Help/Api/GET-api-capabilities-metadataUuid");
            return Redirect(uri);
        }

        /// <summary>
        ///     Get Capabilities from download service
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        [System.Web.Http.Route("capabilities/{metadataUuid}")]
        public IHttpActionResult GetCapabilities(string metadataUuid)
        {
            try
            {
                var capabilities = _capabilitiesService.GetCapabilities(metadataUuid, "v2");
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

        /// <summary>
        ///     Get Projections from download service
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        [System.Web.Http.Route("codelists/projection/{metadataUuid}")]
        public IHttpActionResult GetProjections(string metadataUuid)
        {
            try
            {
                return Ok(_capabilitiesService.GetProjections(metadataUuid));
            }
            catch (Exception ex)
            {
                Log.Error("Error getting projections for uuid: " + metadataUuid, ex);
                return InternalServerError();
            }
        }

        /// <summary>
        ///     Get Areas from download service
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        [System.Web.Http.Route("codelists/area/{metadataUuid}")]
        public IHttpActionResult GetAreas(string metadataUuid)
        {
            try
            {
                return Ok(_capabilitiesService.GetAreas(metadataUuid));
            }
            catch (Exception ex)
            {
                Log.Error("Error getting areas for uuid: " + metadataUuid, ex);
                return InternalServerError();
            }
        }

        /// <summary>
        ///     Get Format from download service
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        [System.Web.Http.Route("codelists/format/{metadataUuid}")]
        public IHttpActionResult GetFormats(string metadataUuid)
        {
            try
            {
                return Ok(_capabilitiesService.GetFormats(metadataUuid));
            }
            catch (Exception ex)
            {
                Log.Error("Error getting formats for uuid: " + metadataUuid, ex);
                return InternalServerError();
            }
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("can-download")]
        public IHttpActionResult CanDownload(CanDownloadRequestType request)
        {
            try
            {
                var canDownload = true;

                if (ConfigurationManager.AppSettings["FmeAreaCheckerEnabled"].Equals("true"))
                    canDownload = _downloadService.AreaIsWithinDownloadLimits(request.coordinates,
                        request.coordinateSystem, request.metadataUuid);

                return Ok(new CanDownloadResponseType() {canDownload = canDownload});
            }
            catch (Exception ex)
            {
                Log.Error("Error returning canDownload for uuid: " + request.metadataUuid, ex);
                return InternalServerError();
            }
        }


    }
}