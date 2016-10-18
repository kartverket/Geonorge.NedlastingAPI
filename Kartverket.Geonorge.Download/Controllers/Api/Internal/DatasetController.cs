using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DatasetController : ApiController
    {
        private DownloadContext db = new DownloadContext();

        /// <summary>
        /// Get info all datasets
        /// </summary>
        // GET: api/Dataset
        public IEnumerable<Dataset> GetCapabilities()
        {
            return db.Capabilities.ToList();
        }
        /// <summary>
        /// Get info dataset
        /// </summary>
        // GET: api/Dataset/5
        [ResponseType(typeof(Dataset))]
        public IHttpActionResult GetDataset(int id)
        {
            Dataset dataset = db.Capabilities.Find(id);
            if (dataset == null)
            {
                return NotFound();
            }

            return Ok(dataset);
        }
        /// <summary>
        /// Update dataset
        /// </summary>
        // PUT: api/Dataset/5
        [Authorize(Users = "download")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutDataset(int id, Dataset dataset)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != dataset.ID)
            {
                return BadRequest();
            }

            db.Entry(dataset).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DatasetExists(id))
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
        // POST: api/Dataset
        [Authorize(Users = "download")]
        [ResponseType(typeof(Dataset))]
        public IHttpActionResult PostDataset(Dataset dataset)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Capabilities.Add(dataset);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = dataset.ID }, dataset);
        }

        /// <summary>
        /// Delete dataset
        /// </summary>
        // DELETE: api/Dataset/5
        [Authorize(Users = "download")]
        [ResponseType(typeof(Dataset))]
        public IHttpActionResult DeleteDataset(int id)
        {
            Dataset dataset = db.Capabilities.Find(id);
            if (dataset == null)
            {
                return NotFound();
            }

            db.Capabilities.Remove(dataset);
            db.SaveChanges();

            return Ok(dataset);
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
}