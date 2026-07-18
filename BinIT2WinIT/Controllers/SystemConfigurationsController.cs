using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BinIT2WinIT.Models;
using SmartRecycling.Data;

namespace BinIT2WinIT.Controllers
{
    public class SystemConfigurationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: SystemConfigurations
        public ActionResult Index()
        {
            return View(db.SystemConfigurations.ToList());
        }

        // GET: SystemConfigurations/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SystemConfiguration systemConfiguration = db.SystemConfigurations.Find(id);
            if (systemConfiguration == null)
            {
                return HttpNotFound();
            }
            return View(systemConfiguration);
        }

        // GET: SystemConfigurations/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SystemConfigurations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ConfigId,ConfigKey,ConfigValue,Description,UpdatedDate,UpdatedBy")] SystemConfiguration systemConfiguration)
        {
            if (ModelState.IsValid)
            {
                db.SystemConfigurations.Add(systemConfiguration);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(systemConfiguration);
        }

        // GET: SystemConfigurations/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SystemConfiguration systemConfiguration = db.SystemConfigurations.Find(id);
            if (systemConfiguration == null)
            {
                return HttpNotFound();
            }
            return View(systemConfiguration);
        }

        // POST: SystemConfigurations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ConfigId,ConfigKey,ConfigValue,Description,UpdatedDate,UpdatedBy")] SystemConfiguration systemConfiguration)
        {
            if (ModelState.IsValid)
            {
                db.Entry(systemConfiguration).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(systemConfiguration);
        }

        // GET: SystemConfigurations/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SystemConfiguration systemConfiguration = db.SystemConfigurations.Find(id);
            if (systemConfiguration == null)
            {
                return HttpNotFound();
            }
            return View(systemConfiguration);
        }

        // POST: SystemConfigurations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SystemConfiguration systemConfiguration = db.SystemConfigurations.Find(id);
            db.SystemConfigurations.Remove(systemConfiguration);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
