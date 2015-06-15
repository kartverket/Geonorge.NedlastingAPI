using Kartverket.Geonorge.Download.Services;
using Kartverket.Geonorge.Download.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Kartverket.Geonorge.Download.Controllers
{
    public class CapabilitiesController : ApiController
    {
        /// <summary>
        /// Get Capabilities from download service
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        [Route("api/capabilities/{metadataUuid}")]
        public CapabilitiesModel GetCapabilities(string metadataUuid)
        {
            return new CapabilitiesService().GetCapabilities(metadataUuid);
        }

        /// <summary>
        /// Get Projections from download service
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        [Route("api/codelists/projection/{metadataUuid}")]
        public ProjectionsModel GetProjections(string metadataUuid)
        {
            return new CapabilitiesService().GetProjections(metadataUuid);
        }

        /// <summary>
        /// Get Areas from download service
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        [Route("api/codelists/area/{metadataUuid}")]
        public AreasModel GetAreas(string metadataUuid)
        {
            return new CapabilitiesService().GetAreas(metadataUuid);
        }

        /// <summary>
        /// Get Format from download service
        /// </summary>
        /// <param name="metadataUuid">The metadata identifier</param>
        [Route("api/codelists/format/{metadataUuid}")]
        public FormatModel GetFormats(string metadataUuid)
        {
            return new CapabilitiesService().GetFormats(metadataUuid);
        }

    }
}
