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
    public class CO2FactorController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: CO2Factor
        public ActionResult Index()
        {
            var cO2Factors = db.CO2Factors.Include(c => c.MaterialType);
            return View(cO2Factors.ToList());
        }

        // GET: CO2Factor/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CO2Factor cO2Factor = db.CO2Factors.Find(id);
            if (cO2Factor == null)
            {
                return HttpNotFound();
            }
            return View(cO2Factor);
        }

        // GET: CO2Factor/Create
        public ActionResult Create()
        {
            ViewBag.MaterialTypeId = new SelectList(db.MaterialTypes, "MaterialTypeId", "Name");
            return View();
        }

        // POST: CO2Factor/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CO2FactorId,MaterialTypeId,CO2SavedPerKg,EffectiveDate,EndDate,IsActive")] CO2Factor cO2Factor)
        {
            if (ModelState.IsValid)
            {
                db.CO2Factors.Add(cO2Factor);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MaterialTypeId = new SelectList(db.MaterialTypes, "MaterialTypeId", "Name", cO2Factor.MaterialTypeId);
            return View(cO2Factor);
        }

        // GET: CO2Factor/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CO2Factor cO2Factor = db.CO2Factors.Find(id);
            if (cO2Factor == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaterialTypeId = new SelectList(db.MaterialTypes, "MaterialTypeId", "Name", cO2Factor.MaterialTypeId);
            return View(cO2Factor);
        }

        // POST: CO2Factor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CO2FactorId,MaterialTypeId,CO2SavedPerKg,EffectiveDate,EndDate,IsActive")] CO2Factor cO2Factor)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cO2Factor).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MaterialTypeId = new SelectList(db.MaterialTypes, "MaterialTypeId", "Name", cO2Factor.MaterialTypeId);
            return View(cO2Factor);
        }

        // GET: CO2Factor/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CO2Factor cO2Factor = db.CO2Factors.Find(id);
            if (cO2Factor == null)
            {
                return HttpNotFound();
            }
            return View(cO2Factor);
        }

        // POST: CO2Factor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CO2Factor cO2Factor = db.CO2Factors.Find(id);
            db.CO2Factors.Remove(cO2Factor);
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
