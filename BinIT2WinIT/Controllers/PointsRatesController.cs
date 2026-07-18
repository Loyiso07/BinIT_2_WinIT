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
    public class PointsRatesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: PointsRates
        public ActionResult Index()
        {
            var pointsRates = db.PointsRates.Include(p => p.MaterialType);
            return View(pointsRates.ToList());
        }

        // GET: PointsRates/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PointsRate pointsRate = db.PointsRates.Find(id);
            if (pointsRate == null)
            {
                return HttpNotFound();
            }
            return View(pointsRate);
        }

        // GET: PointsRates/Create
        public ActionResult Create()
        {
            ViewBag.MaterialTypeId = new SelectList(db.MaterialTypes, "MaterialTypeId", "Name");
            return View();
        }

        // POST: PointsRates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PointsRateId,MaterialTypeId,PointsPerKg,EffectiveDate,EndDate,IsActive")] PointsRate pointsRate)
        {
            if (ModelState.IsValid)
            {
                db.PointsRates.Add(pointsRate);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MaterialTypeId = new SelectList(db.MaterialTypes, "MaterialTypeId", "Name", pointsRate.MaterialTypeId);
            return View(pointsRate);
        }

        // GET: PointsRates/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PointsRate pointsRate = db.PointsRates.Find(id);
            if (pointsRate == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaterialTypeId = new SelectList(db.MaterialTypes, "MaterialTypeId", "Name", pointsRate.MaterialTypeId);
            return View(pointsRate);
        }

        // POST: PointsRates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PointsRateId,MaterialTypeId,PointsPerKg,EffectiveDate,EndDate,IsActive")] PointsRate pointsRate)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pointsRate).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MaterialTypeId = new SelectList(db.MaterialTypes, "MaterialTypeId", "Name", pointsRate.MaterialTypeId);
            return View(pointsRate);
        }

        // GET: PointsRates/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PointsRate pointsRate = db.PointsRates.Find(id);
            if (pointsRate == null)
            {
                return HttpNotFound();
            }
            return View(pointsRate);
        }

        // POST: PointsRates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PointsRate pointsRate = db.PointsRates.Find(id);
            db.PointsRates.Remove(pointsRate);
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
