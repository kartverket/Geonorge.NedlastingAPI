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
        /// Get info all datasets
        /// </summary>
        // GET: api/internal/dataset
        [Route("")]
        public IEnumerable<Dataset> GetCapabilities()
        {
            return db.Capabilities.ToList();
        }
        /// <summary>
        /// Get info dataset
        /// </summary>
        // GET: api/internal/dataset/5
        [ResponseType(typeof(Dataset))]
        [Route("{id:int}")]
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
        // PUT: api/internal/dataset/5
        [Authorize(Users = "download")]
        [Route("")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutDataset(Dataset dataset)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
        [Route("")]
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
        // DELETE: api/internal/dataset/5
        [Authorize(Users = "download")]
        [ResponseType(typeof(Dataset))]
        [Route("{id:int}")]
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