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
    public class PointsTransactionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: PointsTransactions
        public ActionResult Index()
        {
            var pointsTransactions = db.PointsTransactions.Include(p => p.Resident);
            return View(pointsTransactions.ToList());
        }

        // GET: PointsTransactions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PointsTransaction pointsTransaction = db.PointsTransactions.Find(id);
            if (pointsTransaction == null)
            {
                return HttpNotFound();
            }
            return View(pointsTransaction);
        }

        // GET: PointsTransactions/Create
        public ActionResult Create()
        {
            ViewBag.ResidentId = new SelectList(db.Residents, "ResidentId", "UserId");
            return View();
        }

        // POST: PointsTransactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TransactionId,ResidentId,TransactionDate,Amount,Description,Type,ReferenceId,Reason")] PointsTransaction pointsTransaction)
        {
            if (ModelState.IsValid)
            {
                db.PointsTransactions.Add(pointsTransaction);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ResidentId = new SelectList(db.Residents, "ResidentId", "UserId", pointsTransaction.ResidentId);
            return View(pointsTransaction);
        }

        // GET: PointsTransactions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PointsTransaction pointsTransaction = db.PointsTransactions.Find(id);
            if (pointsTransaction == null)
            {
                return HttpNotFound();
            }
            ViewBag.ResidentId = new SelectList(db.Residents, "ResidentId", "UserId", pointsTransaction.ResidentId);
            return View(pointsTransaction);
        }

        // POST: PointsTransactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TransactionId,ResidentId,TransactionDate,Amount,Description,Type,ReferenceId,Reason")] PointsTransaction pointsTransaction)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pointsTransaction).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ResidentId = new SelectList(db.Residents, "ResidentId", "UserId", pointsTransaction.ResidentId);
            return View(pointsTransaction);
        }

        // GET: PointsTransactions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PointsTransaction pointsTransaction = db.PointsTransactions.Find(id);
            if (pointsTransaction == null)
            {
                return HttpNotFound();
            }
            return View(pointsTransaction);
        }

        // POST: PointsTransactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PointsTransaction pointsTransaction = db.PointsTransactions.Find(id);
            db.PointsTransactions.Remove(pointsTransaction);
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
