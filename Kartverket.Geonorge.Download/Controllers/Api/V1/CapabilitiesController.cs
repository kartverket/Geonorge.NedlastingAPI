using System;
using System.Collections.Generic;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;

namespace Kartverket.Geonorge.Download.Controllers.Api.V1
{
    [System.Web.Mvc.HandleError]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class CapabilitiesController : ApiController
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        [Route("api/capabilities/")]
        public IHttpActionResult Index()
        {
            System.Uri uri = new System.Uri(WebConfigurationManager.AppSettings["DownloadUrl"] +  "Help/Api/GET-api-capabilities-metadataUuid");
            return Redirect(uri);

        }

        /// <summary>
        /// Get Capabilities from download service
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        [Route("api/capabilities/{metadataUuid}")]
        public CapabilitiesType GetCapabilities(string metadataUuid)
        {
            try 
            {
                return new CapabilitiesService().GetCapabilities(metadataUuid, "v1");
            }
            catch (Exception ex)
            {
                Log.Error("Error API", ex);
                return null;
            }
        }

        /// <summary>
        /// Get Projections from download service
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        [Route("api/codelists/projection/{metadataUuid}")]
        public List<ProjectionType> GetProjections(string metadataUuid)
        { 
            try 
            { 
                return new CapabilitiesService().GetProjections(metadataUuid);
            }
            catch (Exception ex)
            {
                Log.Error("Error API", ex);
                return null;
            }
        }

        /// <summary>
        /// Get Areas from download service
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        [Route("api/codelists/area/{metadataUuid}")]
        public List<AreaType> GetAreas(string metadataUuid)
        {
            //try
            //{
                return new CapabilitiesService().GetAreas(metadataUuid);
            //}
            //catch (Exception ex)
            //{
            //    Log.Error("Error API", ex);
            //    return null;
            //}
        }

        /// <summary>
        /// Get Format from download service
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        [Route("api/codelists/format/{metadataUuid}")]
        public List<FormatType> GetFormats(string metadataUuid)
        {
            try
            {
                return new CapabilitiesService().GetFormats(metadataUuid);
            }
            catch (Exception ex)
            {
                Log.Error("Error API", ex);
                return null;
            }
        }

    }
}
