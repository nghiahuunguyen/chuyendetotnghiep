using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using chuyende.Models;

namespace chuyende.Areas.Admin.Controllers
{
    public class HoaDonsController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        // GET: Admin/HoaDons
        public ActionResult Index()
        {
            return View(db.HoaDons.ToList());
        }

        // GET: Admin/HoaDons/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HoaDon hoaDon = db.HoaDons.Find(id);
            if (hoaDon == null)
            {
                return HttpNotFound();
            }
            return View(hoaDon);
        }

        // GET: Admin/HoaDons/Create
        public ActionResult Create()
        {
            // Lấy danh sách sản phẩm có SoLuong > 0 và Status == 1
            ViewBag.SanPhams = db.SanPhams
                .Where(s => s.SoLuong > 0 && s.Status == 1)
                .ToList();
            return View();
        }

        // POST: Admin/HoaDons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HoaDon hoaDon, string[] MaSPs, int[] SoLuongs)
        {
            // Kiểm tra nếu ModelState không hợp lệ
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Debug.WriteLine("Error: " + error.ErrorMessage);
                }
            }

            if (ModelState.IsValid)
            {
                hoaDon.MaHD = Guid.NewGuid().ToString("N");
                hoaDon.NgayTao = DateTime.Now;
                hoaDon.NguoiTao = User.Identity.Name;
                hoaDon.TrangThai = 0;

                db.HoaDons.Add(hoaDon);

                for (int i = 0; i < MaSPs.Length; i++)
                {
                    if (SoLuongs[i] > 0)
                    {
                        var cthd = new ChiTietHoaDon
                        {
                            ID = Guid.NewGuid().ToString("N"),
                            MaHD = hoaDon.MaHD,
                            MaSP = MaSPs[i],
                            SoLuong = SoLuongs[i]
                        };
                        db.ChiTietHoaDons.Add(cthd);
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Nếu ModelState không hợp lệ, trả lại view với dữ liệu
            ViewBag.SanPhams = db.SanPhams
                .Where(s => s.SoLuong > 0 && s.Status == 1)
                .ToList();
            return View(hoaDon);
        }


        // GET: Admin/HoaDons/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HoaDon hoaDon = db.HoaDons.Find(id);
            if (hoaDon == null)
            {
                return HttpNotFound();
            }
            return View(hoaDon);
        }

        // POST: Admin/HoaDons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaHD,TenKH,SoDienThoai,Email,DiaChi,PhuongThucThanhToan,TrangThai,NguoiTao,NgayTao")] HoaDon hoaDon)
        {
            if (ModelState.IsValid)
            {
                db.Entry(hoaDon).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(hoaDon);
        }

        // GET: Admin/HoaDons/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HoaDon hoaDon = db.HoaDons.Find(id);
            if (hoaDon == null)
            {
                return HttpNotFound();
            }
            return View(hoaDon);
        }

        // POST: Admin/HoaDons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            HoaDon hoaDon = db.HoaDons.Find(id);
            db.HoaDons.Remove(hoaDon);
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
