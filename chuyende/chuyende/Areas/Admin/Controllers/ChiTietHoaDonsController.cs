using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using chuyende.Models;

namespace chuyende.Areas.Admin.Controllers
{
    public class ChiTietHoaDonsController : Controller
    {
        private QuanLyBanDienTuEntities1 db = new QuanLyBanDienTuEntities1();

        // GET: Admin/ChiTietHoaDons
        public ActionResult Index()
        {
            var chiTietHoaDon = db.ChiTietHoaDon.Include(c => c.HoaDon).Include(c => c.SanPham);
            return View(chiTietHoaDon.ToList());
        }

        // GET: Admin/ChiTietHoaDons/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChiTietHoaDon chiTietHoaDon = db.ChiTietHoaDon.Find(id);
            if (chiTietHoaDon == null)
            {
                return HttpNotFound();
            }
            return View(chiTietHoaDon);
        }

        // GET: Admin/ChiTietHoaDons/Create
        public ActionResult Create()
        {
            ViewBag.MaHD = new SelectList(db.HoaDon, "MaHD", "MaKH");
            ViewBag.MaSP = new SelectList(db.SanPham, "MaSP", "MaLoaiSP");
            return View();
        }

        // POST: Admin/ChiTietHoaDons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaHD,MaSP,SoLuong")] ChiTietHoaDon chiTietHoaDon)
        {
            if (ModelState.IsValid)
            {
                db.ChiTietHoaDon.Add(chiTietHoaDon);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MaHD = new SelectList(db.HoaDon, "MaHD", "MaKH", chiTietHoaDon.MaHD);
            ViewBag.MaSP = new SelectList(db.SanPham, "MaSP", "MaLoaiSP", chiTietHoaDon.MaSP);
            return View(chiTietHoaDon);
        }

        // GET: Admin/ChiTietHoaDons/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChiTietHoaDon chiTietHoaDon = db.ChiTietHoaDon.Find(id);
            if (chiTietHoaDon == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaHD = new SelectList(db.HoaDon, "MaHD", "MaKH", chiTietHoaDon.MaHD);
            ViewBag.MaSP = new SelectList(db.SanPham, "MaSP", "MaLoaiSP", chiTietHoaDon.MaSP);
            return View(chiTietHoaDon);
        }

        // POST: Admin/ChiTietHoaDons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaHD,MaSP,SoLuong")] ChiTietHoaDon chiTietHoaDon)
        {
            if (ModelState.IsValid)
            {
                db.Entry(chiTietHoaDon).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MaHD = new SelectList(db.HoaDon, "MaHD", "MaKH", chiTietHoaDon.MaHD);
            ViewBag.MaSP = new SelectList(db.SanPham, "MaSP", "MaLoaiSP", chiTietHoaDon.MaSP);
            return View(chiTietHoaDon);
        }

        // GET: Admin/ChiTietHoaDons/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChiTietHoaDon chiTietHoaDon = db.ChiTietHoaDon.Find(id);
            if (chiTietHoaDon == null)
            {
                return HttpNotFound();
            }
            return View(chiTietHoaDon);
        }

        // POST: Admin/ChiTietHoaDons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            ChiTietHoaDon chiTietHoaDon = db.ChiTietHoaDon.Find(id);
            db.ChiTietHoaDon.Remove(chiTietHoaDon);
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
