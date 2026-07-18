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
    public class DropOffPointsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: DropOffPoints
        public ActionResult Index()
        {
            return View(db.DropOffPoints.ToList());
        }

        // GET: DropOffPoints/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DropOffPoint dropOffPoint = db.DropOffPoints.Find(id);
            if (dropOffPoint == null)
            {
                return HttpNotFound();
            }
            return View(dropOffPoint);
        }

        // GET: DropOffPoints/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DropOffPoints/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "DropOffPointId,Name,Address,City,Suburb,ContactPerson,PhoneNumber,IsActive,CreatedAt")] DropOffPoint dropOffPoint)
        {
            if (ModelState.IsValid)
            {
                db.DropOffPoints.Add(dropOffPoint);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(dropOffPoint);
        }

        // GET: DropOffPoints/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DropOffPoint dropOffPoint = db.DropOffPoints.Find(id);
            if (dropOffPoint == null)
            {
                return HttpNotFound();
            }
            return View(dropOffPoint);
        }

        // POST: DropOffPoints/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "DropOffPointId,Name,Address,City,Suburb,ContactPerson,PhoneNumber,IsActive,CreatedAt")] DropOffPoint dropOffPoint)
        {
            if (ModelState.IsValid)
            {
                db.Entry(dropOffPoint).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(dropOffPoint);
        }

        // GET: DropOffPoints/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DropOffPoint dropOffPoint = db.DropOffPoints.Find(id);
            if (dropOffPoint == null)
            {
                return HttpNotFound();
            }
            return View(dropOffPoint);
        }

        // POST: DropOffPoints/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DropOffPoint dropOffPoint = db.DropOffPoints.Find(id);
            db.DropOffPoints.Remove(dropOffPoint);
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
