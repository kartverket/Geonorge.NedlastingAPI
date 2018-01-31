using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Utilities;
using ScottBrady91.AspNet.Identity.ConfigurablePasswordHasher;

namespace Kartverket.Geonorge.Download.Controllers
{
    [BaatAuthorization(Role = SecurityClaim.Role.MetadataAdmin)]
    public class MachineAccountsController : Controller
    {
        private readonly DownloadContext _dbContext;

        public MachineAccountsController(DownloadContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: MachineAccounts
        public ActionResult Index()
        {
            return View(_dbContext.MachineAccounts.ToList());
        }

        // GET: MachineAccounts/Details/5
        public ActionResult Details(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var machineAccount = _dbContext.MachineAccounts.Find(id);
            if (machineAccount == null) return HttpNotFound();
            return View(machineAccount);
        }

        // GET: MachineAccounts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: MachineAccounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Username,Passsword,Company,ContactPerson,ContactEmail,Created")]
            MachineAccount machineAccount)
        {
            if (ModelState.IsValid)
            {
                // https://www.scottbrady91.com/ASPNET-Identity/ASPNET-Identity-2-Configurable-Password-Hasher
                machineAccount.Passsword = new ConfigurablePasswordHasher().HashPassword(machineAccount.Passsword);
                
                machineAccount.Created = DateTime.Now;
                
                _dbContext.MachineAccounts.Add(machineAccount);
                _dbContext.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(machineAccount);
        }

        // GET: MachineAccounts/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var machineAccount = _dbContext.MachineAccounts.Find(id);
            if (machineAccount == null) return HttpNotFound();
            return View(machineAccount);
        }

        // POST: MachineAccounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Username,Passsword,Company,ContactPerson,ContactEmail,Created")]
            MachineAccount machineAccount)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Entry(machineAccount).State = EntityState.Modified;
                _dbContext.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(machineAccount);
        }

        // GET: MachineAccounts/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var machineAccount = _dbContext.MachineAccounts.Find(id);
            if (machineAccount == null) return HttpNotFound();
            return View(machineAccount);
        }

        // POST: MachineAccounts/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var machineAccount = _dbContext.MachineAccounts.Find(id);
            _dbContext.MachineAccounts.Remove(machineAccount);
            _dbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _dbContext.Dispose();
            base.Dispose(disposing);
        }
    }
}