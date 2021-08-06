using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using Geonorge.NedlastingApi.V3;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;
using log4net;
using Microsoft.Web.Http;
using WebApi.OutputCache.V2.TimeAttributes;

namespace Kartverket.Geonorge.Download.Controllers.Api.V3
{
    [ApiVersion("3.0")]
    [EnableCors("*", "*", "*")]
    [RoutePrefix("api")]
    public class CapabilitiesV3Controller : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ICapabilitiesService _capabilitiesService;
        private readonly IDownloadService _downloadService;


        public CapabilitiesV3Controller(ICapabilitiesService capabilitiesService, IDownloadService downloadService)
        {
            _capabilitiesService = capabilitiesService;
            _downloadService = downloadService;
        }

        [GeonorgeCacheOutput(ClientTimeSpan = 2592000, ServerTimeSpan = 2592000)] // 30 days cache
        [Route("capabilities/{metadataUuid}")]
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


        /// <summary>
        ///     Get Projections from download service
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        [GeonorgeCacheOutput(ClientTimeSpan = 2592000, ServerTimeSpan = 2592000)] // 30 days cache
        [ResponseType(typeof(List<ProjectionType>))]
        [Route("codelists/projection/{metadataUuid}")]
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
        [GeonorgeCacheOutput(ClientTimeSpan = 2592000, ServerTimeSpan = 2592000)] // 30 days cache
        [Route("codelists/area/{metadataUuid}")]
        [ResponseType(typeof(List<AreaType>))]
        public IHttpActionResult GetAreas(string metadataUuid, string access_token = null)
        {
            try
            {
                return Ok(_capabilitiesService.GetAreas(metadataUuid, ControllerContext.Request));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error("UnauthorizedAccessException: " + metadataUuid, ex);
                return Unauthorized();
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
        [GeonorgeCacheOutput(ClientTimeSpan = 2592000, ServerTimeSpan = 2592000)] // 30 days cache
        [Route("codelists/format/{metadataUuid}")]
        [ResponseType(typeof(List<FormatType>))]
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

        /// <summary>
        ///     If polygon is selected, checks if coordinates is within the maximum allowable area that can be downloaded
        /// </summary>
        [HttpPost]
        [Route("can-download")]
        [ResponseType(typeof(CanDownloadResponseType))]
        public IHttpActionResult CanDownload(CanDownloadRequestType request)
        {
            try
            {
                var canDownload = true;

                if (ConfigurationManager.AppSettings["FmeAreaCheckerEnabled"].Equals("true"))
                    canDownload = _downloadService.AreaIsWithinDownloadLimits(request.coordinates,
                        request.coordinateSystem, request.metadataUuid);

                return Ok(new CanDownloadResponseType { canDownload = canDownload });
            }
            catch (Exception ex)
            {
                Log.Error("Error returning canDownload for uuid: " + request.metadataUuid, ex);
                return InternalServerError();
            }
        }


        [Route("tilgangskontrollmatrikkeleiendomtest/{baatid}")]
        [ResponseType(typeof(List<Eiendom>))]
        public List<Eiendom> GetEiendomTest(string baatid)
        {
            var request = ControllerContext.Request;
            var auth = request.Headers.Authorization;

            List<Eiendom> eiendoms = new List<Eiendom>();
            eiendoms.Add(new Eiendom { kommunenr = "3021", gaardsnr="1",bruksnr="1",festenr="0" });
            eiendoms.Add(new Eiendom { kommunenr = "3021", gaardsnr = "20", bruksnr = "1", festenr = "0" });
            eiendoms.Add(new Eiendom { kommunenr = "3817", gaardsnr = "1", bruksnr = "1", festenr = "0" });

            return eiendoms;
        }
    }
}