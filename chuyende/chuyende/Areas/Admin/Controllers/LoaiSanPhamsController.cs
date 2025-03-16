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
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        // GET: Admin/LoaiSanPhams
        public ActionResult Index()
        {
            //if (Session["User"] == null)
            //{
            //    return RedirectToAction("Index", "DangNhap");
            //}
            return View(db.LoaiSanPhams.ToList());
        }

        // GET: Admin/LoaiSanPhams/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy mã loại sản phẩm.";
                return RedirectToAction("Index");
            }
            LoaiSanPham loaiSanPham = db.LoaiSanPhams.Find(id);
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
        public ActionResult Create([Bind(Include = "TenLoaiSP")] LoaiSanPham loaiSanPham)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy mã loại sản phẩm lớn nhất hiện tại theo format "LSP001"
                    var lastLoaiSP = db.LoaiSanPhams
                                       .Where(lsp => lsp.MaLoaiSP.StartsWith("LSP"))
                                       .OrderByDescending(lsp => lsp.MaLoaiSP)
                                       .Select(lsp => lsp.MaLoaiSP)
                                       .FirstOrDefault();

                    int newId = 1; // Mặc định nếu bảng rỗng

                    if (!string.IsNullOrEmpty(lastLoaiSP) && lastLoaiSP.Length >= 6)
                    {
                        string numberPart = lastLoaiSP.Substring(3); // Lấy phần số
                        if (int.TryParse(numberPart, out int lastId))
                        {
                            newId = lastId + 1; // Tăng lên 1
                        }
                    }

                    loaiSanPham.MaLoaiSP = "LSP" + newId.ToString("D3"); // Format LSP001, LSP002,...

                    db.LoaiSanPhams.Add(loaiSanPham);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Thêm loại sản phẩm thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Thêm loại sản phẩm không thành công. Lỗi: " + ex.Message;
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
            LoaiSanPham loaiSanPham = db.LoaiSanPhams.Find(id);
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
            LoaiSanPham loaiSanPham = db.LoaiSanPhams.Find(id);
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
                LoaiSanPham loaiSanPham = db.LoaiSanPhams.Find(id);
                if (loaiSanPham == null)
                {
                    TempData["ErrorMessage"] = "Loại sản phẩm không tồn tại.";
                    return RedirectToAction("Index");
                }
                db.LoaiSanPhams.Remove(loaiSanPham);
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
