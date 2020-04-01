using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Geonorge.AuthLib.Common;
using Kartverket.Geonorge.Download.Models;
using ScottBrady91.AspNet.Identity.ConfigurablePasswordHasher;

namespace Kartverket.Geonorge.Download.Controllers
{
    [GeoIdAuthorization(Role = GeonorgeRoles.MetadataAdmin)]
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
            ViewBag.Roles = new MultiSelectList(GetAvailableRoles());
            return View();
        }

        private List<string> GetAvailableRoles()
        {
            List<string> roles = new List<string>();
            System.Collections.Specialized.NameValueCollection settings = System.Web.Configuration.WebConfigurationManager.AppSettings;
            string rolesCSV = settings["BasicAuthRoles"];
            if (!string.IsNullOrEmpty(rolesCSV))
            {
                var rolesArray = rolesCSV.Split(',');
                foreach (var role in rolesArray)
                    roles.Add(role);
            }

            return roles;
        }

        // POST: MachineAccounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MachineAccount machineAccount, string[] roles)
        {
            ViewBag.Roles = new MultiSelectList(GetAvailableRoles());

            if (ModelState.IsValid)
            {
                // https://www.scottbrady91.com/ASPNET-Identity/ASPNET-Identity-2-Configurable-Password-Hasher
                machineAccount.Passsword = new ConfigurablePasswordHasher().HashPassword(machineAccount.Passsword);
                
                machineAccount.Created = DateTime.Now;
                if (roles != null)
                    machineAccount.Roles = string.Join(",", roles);
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

            var rolesSelected = machineAccount.Roles != null ? machineAccount.Roles.Split(',').ToArray() : null;
            var rolesAvailable = GetAvailableRoles();
            var rolesData = rolesAvailable.AsEnumerable()
                .Select(c => new SelectListItem
                {
                    Value = c.ToString(),
                    Text = c.ToString(),
                }).ToList();

            ViewBag.RolesData = new MultiSelectList(rolesData, "Value", "Text", rolesSelected);

            machineAccount.Passsword = "";
            return View(machineAccount);
        }

        // POST: MachineAccounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MachineAccount machineAccount, string[] RolesSelected)
        {
            if (ModelState.IsValid)
            {
                var machineAccountUpdated = _dbContext.MachineAccounts.Find(machineAccount.Username);
                machineAccountUpdated.Company = machineAccount.Company;
                machineAccountUpdated.ContactEmail = machineAccount.ContactEmail;
                machineAccountUpdated.ContactPerson = machineAccount.ContactPerson;

                var rolesSelected = RolesSelected;
                var rolesAvailable = GetAvailableRoles();
                var rolesData = rolesAvailable.AsEnumerable()
                    .Select(c => new SelectListItem
                    {
                        Value = c.ToString(),
                        Text = c.ToString(),
                    }).ToList();

                ViewBag.RolesData = new MultiSelectList(rolesData, "Value", "Text", rolesSelected);

                machineAccountUpdated.Roles = rolesSelected != null ? string.Join(",",rolesSelected) : null;

                if (!string.IsNullOrEmpty(machineAccount.Passsword))
                    machineAccountUpdated.Passsword = new ConfigurablePasswordHasher().HashPassword(machineAccount.Passsword);
                _dbContext.Entry(machineAccountUpdated).State = EntityState.Modified;
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