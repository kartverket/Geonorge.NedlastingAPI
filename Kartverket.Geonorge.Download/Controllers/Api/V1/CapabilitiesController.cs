using System;
using System.Collections.Generic;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using Geonorge.NedlastingApi.V3;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;

namespace Kartverket.Geonorge.Download.Controllers.Api.V1
{
    [System.Web.Mvc.HandleError]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class CapabilitiesController : ApiController
    {
        private readonly ICapabilitiesService _capabilitiesService;
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CapabilitiesController(ICapabilitiesService capabilitiesService)
        {
            _capabilitiesService = capabilitiesService;
        }

        /// <summary>
        /// Get Projections from download service
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        [Route("api/codelists/projection/{metadataUuid}")]
        [ResponseType(typeof(List<ProjectionType>))]
        public List<ProjectionType> GetProjections(string metadataUuid)
        { 
            try 
            { 
                return _capabilitiesService.GetProjections(metadataUuid);
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
        [ResponseType(typeof(List<AreaType>))]
        public List<AreaType> GetAreas(string metadataUuid)
        {
            try
            {
                return _capabilitiesService.GetAreas(metadataUuid);
            }
            catch (Exception ex)
            {
                Log.Error("Error API", ex);
                return null;
            }
        }

        /// <summary>
        /// Get Format from download service
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        [Route("api/codelists/format/{metadataUuid}")]
        [ResponseType(typeof(List<FormatType>))]
        public List<FormatType> GetFormats(string metadataUuid)
        {
            try
            {
                return _capabilitiesService.GetFormats(metadataUuid);
            }
            catch (Exception ex)
            {
                Log.Error("Error API", ex);
                return null;
            }
        }

    }
}
