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
    public class KhachHangsController : Controller
    {
        private QuanLyBanDienTuEntities1 db = new QuanLyBanDienTuEntities1();

        public ActionResult Search(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return RedirectToAction("Index"); // Nếu không nhập gì, hiển thị tất cả
            }

            var khachhang = db.KhachHang.FirstOrDefault(h => h.TenKH == keyword || h.SoDienThoai == keyword );

            if (khachhang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hãng nào phù hợp.";
                return RedirectToAction("Index");
            }

            return View("Index", new List<KhachHang> { khachhang }); // Trả về danh sách chỉ có 1 hãng
        }

        // GET: Admin/KhachHangs
        public ActionResult Index()
        {
            return View(db.KhachHang.ToList());
        }

        // GET: Admin/KhachHangs/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khách hàng!";
                return RedirectToAction("Index");
            }
            KhachHang khachHang = db.KhachHang.Find(id);
            if (khachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khách hàng!";
                return RedirectToAction("Index");
            }
            return View(khachHang);
        }

        // GET: Admin/KhachHangs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/KhachHangs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaKH,TenKH,NgaySinh,SoDienThoai,Email,DiaChi,MatKhau")] KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.KhachHang.Add(khachHang);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Thêm khách hàng thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi khi thêm khách hàng!";
                }
            }
            TempData["ErrorMessage"] = "Thêm khách hàng thất bại. Vui lòng kiểm tra lại!";
            return View(khachHang);
        }

        // GET: Admin/KhachHangs/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khách hàng!";
                return RedirectToAction("Index");
            }
            KhachHang khachHang = db.KhachHang.Find(id);
            if (khachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khách hàng!";
                return RedirectToAction("Index");
            }
            return View(khachHang);
        }

        // POST: Admin/KhachHangs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaKH,TenKH,NgaySinh,SoDienThoai,Email,DiaChi,MatKhau")] KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(khachHang).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Chỉnh sửa khách hàng thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi khi chỉnh sửa khách hàng!";
                }
            }
            TempData["ErrorMessage"] = "Chỉnh sửa khách hàng thất bại. Vui lòng kiểm tra lại!";
            return View(khachHang);
        }

        // GET: Admin/KhachHangs/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khách hàng!";
                return RedirectToAction("Index");
            }
            KhachHang khachHang = db.KhachHang.Find(id);
            if (khachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khách hàng!";
                return RedirectToAction("Index");
            }
            return View(khachHang);
        }

        // POST: Admin/KhachHangs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                KhachHang khachHang = db.KhachHang.Find(id);
                db.KhachHang.Remove(khachHang);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Xóa khách hàng thành công!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa khách hàng!";
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
