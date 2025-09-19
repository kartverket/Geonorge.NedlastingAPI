using Asp.Versioning;
using Geonorge.Download.Models;
using Geonorge.Download.Models.Api.Internal;
using Geonorge.Download.Services.Auth;
using Geonorge.Download.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System;

namespace Geonorge.Download.Controllers.Api.Internal
{
    /// <summary>
    /// stuff
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="updateMetadataService"></param>
    /// <param name="cacheStore"></param>
    [ApiController]
    [ApiVersionNeutral]
    [ApiExplorerSettings(GroupName = "internal")]
    [Route("api/internal/metadata")]
    [Authorize(AuthenticationSchemes = BasicAuthHandler.SchemeName, Roles = AuthConfig.DatasetProviderRole)]
    public class MetadataController(ILogger<MetadataController> logger, IUpdateMetadataService updateMetadataService, IOutputCacheStore cacheStore) : ControllerBase
    {

        /// <summary>
        /// Update metadata
        /// </summary>
        [HttpPost("update")]
        public async Task<IActionResult> UpdateMetadata(UpdateMetadataRequest metadata)
        {
            try
            {
                logger.LogInformation($"Update metadata invoked for uuid: {metadata.Uuid}");

                UpdateMetadataInformation metadataInformation = updateMetadataService.Convert(metadata);

                updateMetadataService.UpdateMetadata(metadataInformation);


                await cacheStore.EvictByTagAsync($"meta:{metadata.Uuid.ToString().ToLowerInvariant()}", default);

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