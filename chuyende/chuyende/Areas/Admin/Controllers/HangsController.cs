using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using chuyende.Models;

namespace chuyende.Areas.Admin.Controllers
{
    public class HangsController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();
        public ActionResult Search(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return RedirectToAction("Index"); // Nếu không nhập gì, hiển thị tất cả
            }

            var hang = db.Hangs.FirstOrDefault(h => h.TenHang == keyword || h.TuKhoa == keyword);

            if (hang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hãng nào phù hợp.";
                return RedirectToAction("Index");
            }

            return View("Index", new List<Hang> { hang }); // Trả về danh sách chỉ có 1 hãng
        }

        // GET: Admin/Hangs
        public ActionResult Index()
        {
            //if (Session["User"] == null)
            //{
            //    return RedirectToAction("Index", "DangNhap");
            //}
            return View(db.Hangs.ToList());
        }

        // GET: Admin/NhaCungCaps/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy mã hãng.";
                return RedirectToAction("Index");
            }
            Hang hang = db.Hangs.Find(id);
            if (hang == null)
            {
                TempData["ErrorMessage"] = "Hãng không tồn tại.";
                return RedirectToAction("Index");
            }
            return View(hang);
        }

        // GET: Admin/Hangs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Hangs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaHang,TenHang,Logo,SoDienThoai,Email,DiaChi,TuKhoa")] Hang hang, HttpPostedFileBase Logo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (Logo != null && Logo.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(Logo.FileName);
                        var path = Path.Combine(Server.MapPath("~/img/hang"), fileName);
                        Logo.SaveAs(path);
                        hang.Logo = fileName;
                    }
                    db.Hangs.Add(hang);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Thêm hãng thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm hãng!";
                }
            }
            return View(hang);
        }

        // GET: Admin/Hangs/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Hang hang = db.Hangs.Find(id);
            if (hang == null)
                return HttpNotFound();

            return View(hang);
        }

        // POST: Admin/Hangs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaHang,TenHang,Logo,SoDienThoai,Email,DiaChi,TuKhoa")] Hang hang, HttpPostedFileBase LogoFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingHang = db.Hangs.Find(hang.MaHang);
                    if (existingHang == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy hãng!";
                        return RedirectToAction("Index");
                    }

                    // Xử lý cập nhật ảnh nếu người dùng chọn ảnh mới
                    if (LogoFile != null && LogoFile.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(LogoFile.FileName);
                        var path = Path.Combine(Server.MapPath("~/img/hang"), fileName);
                        LogoFile.SaveAs(path);
                        hang.Logo = fileName; // Cập nhật ảnh mới
                    }
                    else
                    {
                        hang.Logo = existingHang.Logo; // Giữ ảnh cũ nếu không chọn ảnh mới
                    }

                    db.Entry(existingHang).CurrentValues.SetValues(hang);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật hãng thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật hãng!";
                }
            }
            return View(hang);
        }


        // GET: Admin/Hangs/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Hang hang = db.Hangs.Find(id);
            if (hang == null)
                return HttpNotFound();

            return View(hang);
        }

        // POST: Admin/Hangs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                Hang hang = db.Hangs.Find(id);
                db.Hangs.Remove(hang);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Xóa hãng thành công!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Không thể xóa hãng do lỗi dữ liệu hoặc ràng buộc!";
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }


    }
}
