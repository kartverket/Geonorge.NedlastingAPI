using Kartverket.Geonorge.Download.Services;
using Kartverket.Geonorge.Download.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Kartverket.Geonorge.Download.Controllers
{
    [System.Web.Mvc.HandleError]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class CapabilitiesController : ApiController
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Get Capabilities from download service
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        [Route("api/capabilities/{metadataUuid}")]
        public CapabilitiesType GetCapabilities(string metadataUuid)
        {
            try 
            { 
                return new CapabilitiesService().GetCapabilities(metadataUuid);
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
