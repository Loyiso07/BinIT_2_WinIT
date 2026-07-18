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
    public class RecyclingSubmissionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: RecyclingSubmissions
        public ActionResult Index()
        {
            var recyclingSubmissions = db.RecyclingSubmissions.Include(r => r.DropOffPoint).Include(r => r.MaterialType).Include(r => r.Resident).Include(r => r.VerifyingOfficer);
            return View(recyclingSubmissions.ToList());
        }

        // GET: RecyclingSubmissions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RecyclingSubmission recyclingSubmission = db.RecyclingSubmissions.Find(id);
            if (recyclingSubmission == null)
            {
                return HttpNotFound();
            }
            return View(recyclingSubmission);
        }

        // GET: RecyclingSubmissions/Create
        public ActionResult Create()
        {
            ViewBag.DropOffPointId = new SelectList(db.DropOffPoints, "DropOffPointId", "Name");
            ViewBag.MaterialTypeId = new SelectList(db.MaterialTypes, "MaterialTypeId", "Name");
            ViewBag.ResidentId = new SelectList(db.Residents, "ResidentId", "UserId");
            ViewBag.VerifiedBy = new SelectList(db.CollectionOfficers, "OfficerId", "UserId");
            return View();
        }

        // POST: RecyclingSubmissions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SubmissionId,ResidentId,MaterialTypeId,DropOffPointId,Weight,SubmissionDate,Status,OfficerNotes,VerifiedBy,VerifiedDate,IsFlagged,FlagReason")] RecyclingSubmission recyclingSubmission)
        {
            if (ModelState.IsValid)
            {
                db.RecyclingSubmissions.Add(recyclingSubmission);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DropOffPointId = new SelectList(db.DropOffPoints, "DropOffPointId", "Name", recyclingSubmission.DropOffPointId);
            ViewBag.MaterialTypeId = new SelectList(db.MaterialTypes, "MaterialTypeId", "Name", recyclingSubmission.MaterialTypeId);
            ViewBag.ResidentId = new SelectList(db.Residents, "ResidentId", "UserId", recyclingSubmission.ResidentId);
            ViewBag.VerifiedBy = new SelectList(db.CollectionOfficers, "OfficerId", "UserId", recyclingSubmission.VerifiedBy);
            return View(recyclingSubmission);
        }

        // GET: RecyclingSubmissions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RecyclingSubmission recyclingSubmission = db.RecyclingSubmissions.Find(id);
            if (recyclingSubmission == null)
            {
                return HttpNotFound();
            }
            ViewBag.DropOffPointId = new SelectList(db.DropOffPoints, "DropOffPointId", "Name", recyclingSubmission.DropOffPointId);
            ViewBag.MaterialTypeId = new SelectList(db.MaterialTypes, "MaterialTypeId", "Name", recyclingSubmission.MaterialTypeId);
            ViewBag.ResidentId = new SelectList(db.Residents, "ResidentId", "UserId", recyclingSubmission.ResidentId);
            ViewBag.VerifiedBy = new SelectList(db.CollectionOfficers, "OfficerId", "UserId", recyclingSubmission.VerifiedBy);
            return View(recyclingSubmission);
        }

        // POST: RecyclingSubmissions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SubmissionId,ResidentId,MaterialTypeId,DropOffPointId,Weight,SubmissionDate,Status,OfficerNotes,VerifiedBy,VerifiedDate,IsFlagged,FlagReason")] RecyclingSubmission recyclingSubmission)
        {
            if (ModelState.IsValid)
            {
                db.Entry(recyclingSubmission).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DropOffPointId = new SelectList(db.DropOffPoints, "DropOffPointId", "Name", recyclingSubmission.DropOffPointId);
            ViewBag.MaterialTypeId = new SelectList(db.MaterialTypes, "MaterialTypeId", "Name", recyclingSubmission.MaterialTypeId);
            ViewBag.ResidentId = new SelectList(db.Residents, "ResidentId", "UserId", recyclingSubmission.ResidentId);
            ViewBag.VerifiedBy = new SelectList(db.CollectionOfficers, "OfficerId", "UserId", recyclingSubmission.VerifiedBy);
            return View(recyclingSubmission);
        }

        // GET: RecyclingSubmissions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RecyclingSubmission recyclingSubmission = db.RecyclingSubmissions.Find(id);
            if (recyclingSubmission == null)
            {
                return HttpNotFound();
            }
            return View(recyclingSubmission);
        }

        // POST: RecyclingSubmissions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            RecyclingSubmission recyclingSubmission = db.RecyclingSubmissions.Find(id);
            db.RecyclingSubmissions.Remove(recyclingSubmission);
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
