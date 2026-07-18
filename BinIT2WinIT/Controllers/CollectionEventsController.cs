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
    public class CollectionEventsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: CollectionEvents
        public ActionResult Index()
        {
            var collectionEvents = db.CollectionEvents.Include(c => c.DropOffPoint);
            return View(collectionEvents.ToList());
        }

        // GET: CollectionEvents/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CollectionEvent collectionEvent = db.CollectionEvents.Find(id);
            if (collectionEvent == null)
            {
                return HttpNotFound();
            }
            return View(collectionEvent);
        }

        // GET: CollectionEvents/Create
        public ActionResult Create()
        {
            ViewBag.DropOffPointId = new SelectList(db.DropOffPoints, "DropOffPointId", "Name");
            return View();
        }

        // POST: CollectionEvents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EventId,DropOffPointId,EventDate,StartTime,EndTime,Description,IsActive")] CollectionEvent collectionEvent)
        {
            if (ModelState.IsValid)
            {
                db.CollectionEvents.Add(collectionEvent);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DropOffPointId = new SelectList(db.DropOffPoints, "DropOffPointId", "Name", collectionEvent.DropOffPointId);
            return View(collectionEvent);
        }

        // GET: CollectionEvents/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CollectionEvent collectionEvent = db.CollectionEvents.Find(id);
            if (collectionEvent == null)
            {
                return HttpNotFound();
            }
            ViewBag.DropOffPointId = new SelectList(db.DropOffPoints, "DropOffPointId", "Name", collectionEvent.DropOffPointId);
            return View(collectionEvent);
        }

        // POST: CollectionEvents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EventId,DropOffPointId,EventDate,StartTime,EndTime,Description,IsActive")] CollectionEvent collectionEvent)
        {
            if (ModelState.IsValid)
            {
                db.Entry(collectionEvent).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DropOffPointId = new SelectList(db.DropOffPoints, "DropOffPointId", "Name", collectionEvent.DropOffPointId);
            return View(collectionEvent);
        }

        // GET: CollectionEvents/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CollectionEvent collectionEvent = db.CollectionEvents.Find(id);
            if (collectionEvent == null)
            {
                return HttpNotFound();
            }
            return View(collectionEvent);
        }

        // POST: CollectionEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CollectionEvent collectionEvent = db.CollectionEvents.Find(id);
            db.CollectionEvents.Remove(collectionEvent);
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
