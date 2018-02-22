using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Models.Api.Internal;
using Kartverket.Geonorge.Download.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;

namespace Kartverket.Geonorge.Download.Controllers.Api.Internal
{
    [Authorize(Roles = AuthConfig.DatasetProviderRole)]
    [RoutePrefix("api/internal/metadata")]
    public class MetadataController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IUpdateMetadataService _updateMetadataService;

        public MetadataController(IUpdateMetadataService updateMetadataService)
        {
            _updateMetadataService = updateMetadataService;
        }

        /// <summary>
        ///     Update metadata
        /// </summary>
        [Route("update")]
        [HttpPost]
        public IHttpActionResult UpdateMetadata(UpdateMetadataRequest metadata)
        {
            try
            {
                Log.Info($"Update metadata invoked for uuid: {metadata.Uuid}");

                UpdateMetadataInformation metadataInformation = _updateMetadataService.Convert(metadata);

                _updateMetadataService.UpdateMetadata(metadataInformation);
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
                return InternalServerError(e);
            }
            return Ok();
        }
    }
}