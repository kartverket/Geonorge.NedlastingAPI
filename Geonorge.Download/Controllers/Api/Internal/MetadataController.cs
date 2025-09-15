using Geonorge.Download.Controllers.Api.V3;
using Geonorge.Download.Models;
using Geonorge.Download.Models.Api.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Geonorge.Download.Services.Interfaces;
using Asp.Versioning;
using Geonorge.Download.Services.Auth;

namespace Geonorge.Download.Controllers.Api.Internal
{
    /// <summary>
    /// stuff
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="config"></param>
    /// <param name="updateMetadataService"></param>
    [ApiController]
    [ApiVersionNeutral]
    [ApiExplorerSettings(GroupName = "internal")]
    [Route("api/internal/metadata")]
    [Authorize(AuthenticationSchemes = BasicAuthHandler.SchemeName, Roles = AuthConfig.DatasetProviderRole)]
    public class MetadataController(ILogger<MetadataController> logger, IConfiguration config, IUpdateMetadataService updateMetadataService) : ControllerBase
    {

        /// <summary>
        /// Update metadata
        /// </summary>
        [HttpPost("update")]
        public IActionResult UpdateMetadata(UpdateMetadataRequest metadata)
        {
            try
            {
                logger.LogInformation($"Update metadata invoked for uuid: {metadata.Uuid}");

                UpdateMetadataInformation metadataInformation = updateMetadataService.Convert(metadata);

                updateMetadataService.UpdateMetadata(metadataInformation);

                // TODO: Fix caching

                //var cache = Configuration.CacheOutputConfiguration().GetCacheOutputProvider(Request);

                //// invalidate cache of "CapabilitiesV3Controller"
                //cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((CapabilitiesV3Controller t) => t.GetCapabilities(metadata.Uuid)));
                //cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((CapabilitiesV3Controller t) => t.GetAreas(metadata.Uuid, null)));
                //cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((CapabilitiesV3Controller t) => t.GetProjections(metadata.Uuid)));
                //cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((CapabilitiesV3Controller t) => t.GetFormats(metadata.Uuid)));

            }
            catch (Exception e)
            {
                logger.LogError(e.Message, e);
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
            return Ok();
        }
    }
}