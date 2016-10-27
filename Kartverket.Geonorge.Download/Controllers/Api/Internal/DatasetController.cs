using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Controllers.Api.Internal
{
    //[ApiExplorerSettings(IgnoreApi = true)]
    [RoutePrefix("api/internal/dataset")]
    public class DatasetController : ApiController
    {
        private DownloadContext db = new DownloadContext();

        /// <summary>
        /// List datasets
        /// </summary>
        // GET: api/internal/dataset
        [Route("")]
        public IEnumerable<DatasetViewModel> GetCapabilities()
        {
            return db.Capabilities.Select(d => new DatasetViewModel { ID = d.ID, metadataUuid = d.metadataUuid, Tittel= d.Tittel }).ToList();
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
        [Authorize(Users = "download")]
        [Route("{uuid:guid}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutDataset(string uuid, Dataset dataset)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if(dataset.filliste.Count > 0)
            {
                //Remove old files
                var deleteSql = "DELETE FROM filliste FROM Dataset INNER JOIN filliste ON Dataset.ID = filliste.dataset WHERE(Dataset.metadataUuid = {0} )";
                db.Database.ExecuteSqlCommand(deleteSql, uuid);

                //Add new files
                foreach (var file in dataset.filliste)
                {
                    db.FileList.Add(file);
                }
                db.SaveChanges();
            }

            db.Entry(dataset).State = EntityState.Modified;

            try
            {
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

            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Create new dataset
        /// </summary>
        // POST: api/internal/dataset
        [Authorize(Users = "download")]
        [ResponseType(typeof(Dataset))]
        [Route]
        public IHttpActionResult PostDataset(Dataset dataset)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Capabilities.Add(dataset);
            db.SaveChanges();

            return Ok(dataset);
        }

        /// <summary>
        /// Delete dataset
        /// </summary>
        // DELETE: api/internal/dataset/db4b872f-264d-434c-9574-57232f1e90d2
        [Authorize(Users = "download")]
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

            db.Capabilities.Remove(dataset);
            db.SaveChanges();

            return Ok(dataset);
        }

        /// <summary>
        /// Delete file
        /// </summary>
        // DELETE: api/internal/dataset/file/Offentligetjenester_2014_Loppa_25835_Skoler_SOSI.zip
        [Authorize(Users = "download")]
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

            db.FileList.Remove(fil);
            db.SaveChanges();

            return Ok(fil);
        }

        /// <summary>
        /// Delete filename list
        /// </summary>
        // DELETE: api/internal/dataset/files
        [Authorize(Users = "download")]
        [Route("files")]
        [HttpDelete]
        public IHttpActionResult DeleteFiles(List<string> filliste)
        {
            foreach (var filnavn in filliste)
            {
                filliste fil = db.FileList.Where(d => d.filnavn == filnavn).FirstOrDefault();
                if (fil == null)
                {
                    return NotFound();
                }

                db.FileList.Remove(fil);
            }
            db.SaveChanges();

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
    }
}