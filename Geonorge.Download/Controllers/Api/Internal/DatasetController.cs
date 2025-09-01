using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Geonorge.Download.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Geonorge.Download.Controllers.Api.Internal
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/internal/dataset")]
    [Authorize(Roles = AuthConfig.DatasetProviderRole)]
    [RequireHttps]
    public class DatasetController(ILogger<DatasetController> logger, DownloadContext downloadContext) : ControllerBase
    {

        /// <summary>
        ///     List datasets
        /// </summary>
        // GET: api/internal/dataset
        [HttpGet]
        [Route("")]
        public IEnumerable<DatasetViewModel> GetCapabilities()
        {
            return downloadContext.Capabilities.Select(d => new DatasetViewModel
            {
                ID = d.Id,
                metadataUuid = d.MetadataUuid,
                Tittel = d.Title,
                AccessConstraint = d.AccessConstraint
            }).ToList();
        }

        /// <summary>
        ///     Get info dataset
        /// </summary>
        // GET: api/internal/dataset/db4b872f-264d-434c-9574-57232f1e90d2
        [HttpGet]        
        [Route("{uuid:guid}")]
        [ProducesResponseType(typeof(Dataset), StatusCodes.Status200OK)]
        public IActionResult GetDataset(string uuid)
        {
            var dataset = downloadContext.Capabilities.Where(d => d.MetadataUuid == uuid).FirstOrDefault();
            if (dataset == null)
                return NotFound();

            return Ok(dataset);
        }

        /// <summary>
        ///     Update dataset
        /// </summary>
        // PUT: api/internal/dataset/db4b872f-264d-434c-9574-57232f1e90d2
        [HttpPut]
        [Route("{uuid:guid}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public IActionResult PutDataset(string uuid, Dataset dataset)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                if (dataset.filliste.Count > 0)
                {
                    //Remove old files
                    var filesToRemove = downloadContext.FileList
                        .Where(f => downloadContext.Capabilities
                            .Any(d => d.Id == f.DatasetId && d.MetadataUuid == uuid))
                        .ToList();

                    if (filesToRemove.Any())
                    {
                        logger.LogInformation("Deleting from filliste for uuid: " + uuid);
                        downloadContext.FileList.RemoveRange(filesToRemove);
                    }

                    //Add new files
                    foreach (var file in dataset.filliste)
                    {
                        logger.LogInformation("Adding file " + file.Filename + " for uuid: " + uuid);
                        downloadContext.FileList.Add(file);
                    }
                    downloadContext.SaveChanges();
                }

                downloadContext.Entry(dataset).State = EntityState.Modified;
                logger.LogInformation("Saving dataset for uuid: " + uuid);

                LogValidationErrors(downloadContext.ChangeTracker);
                downloadContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DatasetExists(dataset.Id))
                    return NotFound();
                throw;
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            
            return NoContent();
        }

        /// <summary>
        ///     Create new dataset
        /// </summary>
        // POST: api/internal/dataset
        [HttpPost]
        [ProducesResponseType(typeof(Dataset), StatusCodes.Status200OK)]
        public IActionResult PostDataset(Dataset dataset)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                downloadContext.Capabilities.Add(dataset);
                logger.LogInformation("Adding new dataset with uuid: " + dataset.MetadataUuid);
                
                LogValidationErrors(downloadContext.ChangeTracker);
                downloadContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(dataset);
        }

        /// <summary>
        ///     Add file(s) to dataset
        /// </summary>
        // POST: api/internal/dataset/files/73f863ba-628f-48af-b7fa-30d3ab331b8d
        [HttpPost]
        [Route("files/{uuid}")]
        public IActionResult PostFiles(string uuid, HashSet<Models.File> filelist)
        {
            try
            {
                var dataset = downloadContext.Capabilities.Where(d => d.MetadataUuid == uuid).FirstOrDefault();
                if (dataset == null)
                    return NotFound();

                foreach (var file in filelist)
                {
                    file.Dataset = null;
                    dataset.filliste.Add(file);
                    logger.LogInformation("Adding file for " + uuid + ": " + file.Filename);
                }
                
                LogValidationErrors(downloadContext.ChangeTracker);
                downloadContext.SaveChanges();

                return Ok(dataset);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        ///     Delete dataset
        /// </summary>
        // DELETE: api/internal/dataset/db4b872f-264d-434c-9574-57232f1e90d2
        [HttpDelete]
        [Route("{uuid:guid}")]
        [ProducesResponseType(typeof(Dataset), StatusCodes.Status200OK)]
        public IActionResult DeleteDataset(string uuid)
        {
            var dataset = downloadContext.Capabilities.Where(d => d.MetadataUuid == uuid).FirstOrDefault();
            if (dataset == null)
                return NotFound();
            try
            {
                downloadContext.Capabilities.Remove(dataset);
                logger.LogInformation("Removing dataset with uuid: " + uuid);
                
                LogValidationErrors(downloadContext.ChangeTracker);
                downloadContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(dataset);
        }

        /// <summary>
        ///     Delete file
        /// </summary>
        // DELETE: api/internal/dataset/file/Offentligetjenester_2014_Loppa_25835_Skoler_SOSI.zip
        [HttpDelete]
        [Route("file/{filnavn}")]
        [ProducesResponseType(typeof(Models.File), StatusCodes.Status200OK)]
        public IActionResult Deletefilliste(string filnavn)
        {
            var fil = downloadContext.FileList.Where(d => d.Filename == filnavn).FirstOrDefault();
            if (fil == null)
                return NotFound();
            try
            {
                downloadContext.FileList.Remove(fil);
                logger.LogInformation("Removing file:" + fil);

                LogValidationErrors(downloadContext.ChangeTracker);
                downloadContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(fil);
        }

        /// <summary>
        ///     Delete filename list
        /// </summary>
        // DELETE: api/internal/dataset/files
        [HttpDelete]
        [Route("files")]
        public IActionResult DeleteFiles(List<string> filliste)
        {
            try
            {
                foreach (var filnavn in filliste)
                {
                    var fil = downloadContext.FileList.Where(d => d.Filename == filnavn).FirstOrDefault();
                    if (fil == null)
                        return NotFound();
                    logger.LogInformation("Deleting file: " + filnavn);
                    downloadContext.FileList.Remove(fil);
                }
                LogValidationErrors(downloadContext.ChangeTracker);
                downloadContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return StatusCode(StatusCodes.Status200OK);
        }

        private bool DatasetExists(int id)
        {
            return downloadContext.Capabilities.Count(e => e.Id == id) > 0;
        }

        private void LogValidationErrors(ChangeTracker tracker)
        {
            var validationErrors = tracker
                    .Entries<IValidatableObject>()
                    .SelectMany(e => e.Entity.Validate(null))
                    .Where(r => r != ValidationResult.Success);
            if (validationErrors.Any())
                foreach (var error in validationErrors)
                    logger.LogError(error.ErrorMessage);
        }
    }

    public class DatasetViewModel
    {
        public int ID { get; set; }
        public string metadataUuid { get; set; }
        public string Tittel { get; set; }
        public string AccessConstraint { get; set; }
    }
}