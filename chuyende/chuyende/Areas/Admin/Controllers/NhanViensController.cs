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
    public class NhanViensController : Controller
    {
        private QuanLyBanDienTuEntities1 db = new QuanLyBanDienTuEntities1();

        public ActionResult Search(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return RedirectToAction("Index"); // Nếu không nhập gì, hiển thị tất cả
            }

            var nhanvien = db.NhanVien.FirstOrDefault(h => h.TenNV == keyword);

            if (nhanvien == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hãng nào phù hợp.";
                return RedirectToAction("Index");
            }

            return View("Index", new List<NhanVien> { nhanvien }); 
        }

        // GET: Admin/NhanViens
        public ActionResult Index()
        {
            var nhanVien = db.NhanVien.Include(n => n.ChucVu);
            return View(nhanVien.ToList());
        }

        // GET: Admin/NhanViens/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy mã nhân viên.";
                return RedirectToAction("Index");
            }
            NhanVien nhanVien = db.NhanVien.Find(id);
            if (nhanVien == null)
            {
                TempData["ErrorMessage"] = "Nhân viên không tồn tại.";
                return RedirectToAction("Index");
            }
            return View(nhanVien);
        }

        // GET: Admin/NhanViens/Create
        public ActionResult Create()
        {
            ViewBag.MaCV = new SelectList(db.ChucVu, "MaCV", "TenCV");
            return View();
        }

        // POST: Admin/NhanViens/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaNV,TenNV,SoDienThoai,Email,NgaySinh,GioiTinh,CCCD,DiaChi,TenDN,MatKhau,MaCV")] NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.NhanVien.Add(nhanVien);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Thêm nhân viên không thành công.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            }

            ViewBag.MaCV = new SelectList(db.ChucVu, "MaCV", "TenCV", nhanVien.MaCV);
            return View(nhanVien);
        }

        // GET: Admin/NhanViens/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy mã nhân viên.";
                return RedirectToAction("Index");
            }
            NhanVien nhanVien = db.NhanVien.Find(id);
            if (nhanVien == null)
            {
                TempData["ErrorMessage"] = "Nhân viên không tồn tại.";
                return RedirectToAction("Index");
            }
            ViewBag.MaCV = new SelectList(db.ChucVu, "MaCV", "TenCV", nhanVien.MaCV);
            return View(nhanVien);
        }

        // POST: Admin/NhanViens/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaNV,TenNV,SoDienThoai,Email,NgaySinh,GioiTinh,CCCD,DiaChi,TenDN,MatKhau,MaCV")] NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(nhanVien).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật thông tin nhân viên thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Cập nhật thông tin nhân viên không thành công.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            }
            ViewBag.MaCV = new SelectList(db.ChucVu, "MaCV", "TenCV", nhanVien.MaCV);
            return View(nhanVien);
        }

        // GET: Admin/NhanViens/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy mã nhân viên.";
                return RedirectToAction("Index");
            }
            NhanVien nhanVien = db.NhanVien.Find(id);
            if (nhanVien == null)
            {
                TempData["ErrorMessage"] = "Nhân viên không tồn tại.";
                return RedirectToAction("Index");
            }
            return View(nhanVien);
        }

        // POST: Admin/NhanViens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                NhanVien nhanVien = db.NhanVien.Find(id);
                if (nhanVien == null)
                {
                    TempData["ErrorMessage"] = "Nhân viên không tồn tại.";
                    return RedirectToAction("Index");
                }
                db.NhanVien.Remove(nhanVien);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Xóa nhân viên thành công!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Xóa nhân viên không thành công.";
            }
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
