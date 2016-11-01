using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Kartverket.Geonorge.Download.Models;
using log4net;
using System.Reflection;
using System;
using System.Data.Entity.Validation;

namespace Kartverket.Geonorge.Download.Controllers.Api.Internal
{
    //[ApiExplorerSettings(IgnoreApi = true)]
    [RoutePrefix("api/internal/dataset")]
    [Authorize(Roles = AuthConfig.DatasetProviderRole)]
    [RequireHttpsNonLocal]
    public class DatasetController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private DownloadContext db = new DownloadContext();

        /// <summary>
        /// List datasets
        /// </summary>
        // GET: api/internal/dataset
        [Route("")]
        public IEnumerable<DatasetViewModel> GetCapabilities()
        {
            return db.Capabilities.Select(d => new DatasetViewModel { ID = d.ID, metadataUuid = d.metadataUuid, Tittel= d.Tittel, AccessConstraint = d.AccessConstraint }).ToList();
        }
        /// <summary>
        /// Get info dataset
        /// </summary>
        // GET: api/internal/dataset/db4b872f-264d-434c-9574-57232f1e90d2
        [ResponseType(typeof(Dataset))]
        [Route("{uuid:guid}")]
        public IHttpActionResult GetDataset(string uuid)
        {
            Dataset dataset = db.Capabilities.Where(d => d.metadataUuid == uuid).FirstOrDefault();
            if (dataset == null)
            {
                return NotFound();
            }

            return Ok(dataset);
        }
        /// <summary>
        /// Update dataset
        /// </summary>
        // PUT: api/internal/dataset/db4b872f-264d-434c-9574-57232f1e90d2
        [Route("{uuid:guid}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutDataset(string uuid, Dataset dataset)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                if (dataset.filliste.Count > 0)
                {
                    //Remove old files
                    var deleteSql = "DELETE FROM filliste FROM Dataset INNER JOIN filliste ON Dataset.ID = filliste.dataset WHERE(Dataset.metadataUuid = {0} )";
                    Log.Info("Deleting from filliste for uuid: " + uuid);
                    db.Database.ExecuteSqlCommand(deleteSql, uuid);

                    //Add new files
                    foreach (var file in dataset.filliste)
                    {
                        Log.Info("Adding file " + file.filnavn + " for uuid: " + uuid);
                        db.FileList.Add(file);

                    }
                    db.SaveChanges();
                }

                db.Entry(dataset).State = EntityState.Modified;
                Log.Info("Saving dataset for uuid: " + uuid);
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DatasetExists(dataset.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var err in ex.EntityValidationErrors)
                {
                    foreach (var e in err.ValidationErrors)
                    {
                        Log.Error(e.ErrorMessage);
                    }
                }
                return StatusCode(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return StatusCode(HttpStatusCode.InternalServerError);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Create new dataset
        /// </summary>
        // POST: api/internal/dataset
        [ResponseType(typeof(Dataset))]
        [Route]
        public IHttpActionResult PostDataset(Dataset dataset)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try { 
            db.Capabilities.Add(dataset);
            Log.Info("Adding new dataset with uuid: " + dataset.metadataUuid);
            db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var err in ex.EntityValidationErrors)
                {
                    foreach (var e in err.ValidationErrors)
                    {
                        Log.Error(e.ErrorMessage);
                    }
                }
                return StatusCode(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return StatusCode(HttpStatusCode.InternalServerError);
            }

            return Ok(dataset);
        }

        /// <summary>
        /// Add file(s) to dataset
        /// </summary>
        // POST: api/internal/dataset/files/73f863ba-628f-48af-b7fa-30d3ab331b8d
        [Route("files/{uuid}")]
        [HttpPost]
        public IHttpActionResult PostFiles(string uuid, HashSet<filliste> filelist)
        {
            try
            {
                Dataset dataset = db.Capabilities.Where(d => d.metadataUuid == uuid).FirstOrDefault();
                if (dataset == null)
                {
                    return NotFound();
                }

                foreach (var file in filelist)
                {
                    file.Dataset1 = null;
                    dataset.filliste.Add(file);
                    Log.Info("Adding file for " + uuid + ": " + file.filnavn);
                }

                db.SaveChanges();

                return Ok(dataset);
            }
            catch (DbEntityValidationException ex)
            {
                foreach(var err in ex.EntityValidationErrors) {
                    foreach(var e in err.ValidationErrors) { 
                    Log.Error(e.ErrorMessage);
                    }
                }
                return StatusCode(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return StatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Delete dataset
        /// </summary>
        // DELETE: api/internal/dataset/db4b872f-264d-434c-9574-57232f1e90d2
        [ResponseType(typeof(Dataset))]
        [Route("{uuid:guid}")]
        [HttpDelete]
        public IHttpActionResult DeleteDataset(string uuid)
        {
            Dataset dataset = db.Capabilities.Where(d => d.metadataUuid == uuid).FirstOrDefault();
            if (dataset == null)
            {
                return NotFound();
            }
            try
            { 
                db.Capabilities.Remove(dataset);
                Log.Info("Removing dataset with uuid: " + uuid);
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var err in ex.EntityValidationErrors)
                {
                    foreach (var e in err.ValidationErrors)
                    {
                        Log.Error(e.ErrorMessage);
                    }
                }
                return StatusCode(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return StatusCode(HttpStatusCode.InternalServerError);
            }

            return Ok(dataset);
        }

        /// <summary>
        /// Delete file
        /// </summary>
        // DELETE: api/internal/dataset/file/Offentligetjenester_2014_Loppa_25835_Skoler_SOSI.zip
        [ResponseType(typeof(filliste))]
        [Route("file/{filnavn}")]
        [HttpDelete]
        public IHttpActionResult Deletefilliste(string filnavn)
        {
            filliste fil = db.FileList.Where(d => d.filnavn == filnavn).FirstOrDefault();
            if (fil == null)
            {
                return NotFound();
            }
            try
            { 
                db.FileList.Remove(fil);
                Log.Info("Removing file:" + fil);
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var err in ex.EntityValidationErrors)
                {
                    foreach (var e in err.ValidationErrors)
                    {
                        Log.Error(e.ErrorMessage);
                    }
                }
                return StatusCode(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return StatusCode(HttpStatusCode.InternalServerError);
            }

            return Ok(fil);
        }

        /// <summary>
        /// Delete filename list
        /// </summary>
        // DELETE: api/internal/dataset/files
        [Route("files")]
        [HttpDelete]
        public IHttpActionResult DeleteFiles(List<string> filliste)
        {
            try
            { 
                foreach (var filnavn in filliste)
                {
                    filliste fil = db.FileList.Where(d => d.filnavn == filnavn).FirstOrDefault();
                    if (fil == null)
                    {
                        return NotFound();
                    }
                    Log.Info("Deleting file: " + filnavn);
                    db.FileList.Remove(fil);
                }
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var err in ex.EntityValidationErrors)
                {
                    foreach (var e in err.ValidationErrors)
                    {
                        Log.Error(e.ErrorMessage);
                    }
                }
                return StatusCode(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return StatusCode(HttpStatusCode.InternalServerError);
            }

            return StatusCode(HttpStatusCode.OK);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool DatasetExists(int id)
        {
            return db.Capabilities.Count(e => e.ID == id) > 0;
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