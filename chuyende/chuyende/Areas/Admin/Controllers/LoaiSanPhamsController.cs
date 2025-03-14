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
    public class LoaiSanPhamsController : Controller
    {
        private QuanLyBanDienTuEntities1 db = new QuanLyBanDienTuEntities1();

        // GET: Admin/LoaiSanPhams
        public ActionResult Index()
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }
            return View(db.LoaiSanPham.ToList());
        }

        // GET: Admin/LoaiSanPhams/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy mã loại sản phẩm.";
                return RedirectToAction("Index");
            }
            LoaiSanPham loaiSanPham = db.LoaiSanPham.Find(id);
            if (loaiSanPham == null)
            {
                TempData["ErrorMessage"] = "Loại sản phẩm không tồn tại.";
                return RedirectToAction("Index");
            }
            return View(loaiSanPham);
        }

        // GET: Admin/LoaiSanPhams/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/LoaiSanPhams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaLoaiSP,TenLoaiSP")] LoaiSanPham loaiSanPham)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.LoaiSanPham.Add(loaiSanPham);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Thêm loại sản phẩm thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Thêm loại sản phẩm không thành công.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            }
            return View(loaiSanPham);
        }

        // GET: Admin/LoaiSanPhams/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy mã loại sản phẩm.";
                return RedirectToAction("Index");
            }
            LoaiSanPham loaiSanPham = db.LoaiSanPham.Find(id);
            if (loaiSanPham == null)
            {
                TempData["ErrorMessage"] = "Loại sản phẩm không tồn tại.";
                return RedirectToAction("Index");
            }
            return View(loaiSanPham);
        }

        // POST: Admin/LoaiSanPhams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaLoaiSP,TenLoaiSP")] LoaiSanPham loaiSanPham)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(loaiSanPham).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật loại sản phẩm thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Cập nhật loại sản phẩm không thành công.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            }
            return View(loaiSanPham);
        }

        // GET: Admin/LoaiSanPhams/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy mã loại sản phẩm.";
                return RedirectToAction("Index");
            }
            LoaiSanPham loaiSanPham = db.LoaiSanPham.Find(id);
            if (loaiSanPham == null)
            {
                TempData["ErrorMessage"] = "Loại sản phẩm không tồn tại.";
                return RedirectToAction("Index");
            }
            return View(loaiSanPham);
        }

        // POST: Admin/LoaiSanPhams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                LoaiSanPham loaiSanPham = db.LoaiSanPham.Find(id);
                if (loaiSanPham == null)
                {
                    TempData["ErrorMessage"] = "Loại sản phẩm không tồn tại.";
                    return RedirectToAction("Index");
                }
                db.LoaiSanPham.Remove(loaiSanPham);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Xóa loại sản phẩm thành công!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Xóa loại sản phẩm không thành công.";
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
