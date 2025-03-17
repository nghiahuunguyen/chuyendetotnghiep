using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using chuyende.Models;

namespace chuyende.Areas.Admin.Controllers
{
    public class ChucVusController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        // Hiển thị danh sách chức vụ
        public ActionResult Index(string status = "Active")
        {
            var chucVus = db.ChucVus.AsQueryable();
            if (status == "Active")
            {
                chucVus = chucVus.Where(m => m.Status == 1);
            }
            else if (status == "Deleted")
            {
                chucVus = chucVus.Where(m => m.Status == 0);
            }
            return View(chucVus.ToList());
        }

        // Xem chi tiết chức vụ
        public ActionResult Details(string id)
        {
            if (id == null) return RedirectToAction("Index");
            ChucVu chucVu = db.ChucVus.Find(id);
            if (chucVu == null) return RedirectToAction("Index");
            return View(chucVu);
        }

        // Hiển thị form thêm mới
        public ActionResult Create() => View();

        // Xử lý thêm mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaCV,TenCV")] ChucVu chucVu)
        {
            if (ModelState.IsValid)
            {
                chucVu.Status = 1;
                db.ChucVus.Add(chucVu);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Chức vụ đã được thêm thành công!";
                return RedirectToAction("Index");
            }
            return View(chucVu);
        }

        // Hiển thị form chỉnh sửa
        public ActionResult Edit(string id)
        {
            if (id == null) return RedirectToAction("Index");
            ChucVu chucVu = db.ChucVus.Find(id);
            if (chucVu == null) return RedirectToAction("Index");
            return View(chucVu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaCV,TenCV")] ChucVu chucVu)
        {
            if (ModelState.IsValid)
            {
                var existingChucVu = db.ChucVus.Find(chucVu.MaCV);
                if (existingChucVu != null)
                {
                    existingChucVu.TenCV = chucVu.TenCV;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Chức vụ đã được cập nhật!";
                }
                return RedirectToAction("Index");
            }
            return View(chucVu);
        }

        // Chuyển chức vụ vào thùng rác
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoveToTrash(string id)
        {
            ChucVu chucVu = db.ChucVus.Find(id);
            if (chucVu != null)
            {
                chucVu.Status = 0;
                db.SaveChanges();
                TempData["WarningMessage"] = "Chức vụ đã được chuyển vào thùng rác.";
            }
            return RedirectToAction("Index");
        }

        // Hiển thị danh sách chức vụ trong thùng rác
        public ActionResult Trash()
        {
            var deletedChucVus = db.ChucVus.Where(m => m.Status == 0).ToList();
            return View(deletedChucVus);
        }

        // Khôi phục chức vụ từ thùng rác
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Restore(string id)
        {
            ChucVu chucVu = db.ChucVus.Find(id);
            if (chucVu != null)
            {
                chucVu.Status = 1;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Chức vụ đã được khôi phục!";
            }
            return RedirectToAction("Trash");
        }

        // Xóa vĩnh viễn một chức vụ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForever(string id)
        {
            ChucVu chucVu = db.ChucVus.Find(id);
            if (chucVu != null)
            {
                db.ChucVus.Remove(chucVu);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Chức vụ đã bị xóa vĩnh viễn!";
            }
            return RedirectToAction("Trash");
        }

        // Khôi phục tất cả chức vụ từ thùng rác
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RestoreAll()
        {
            var deletedChucVus = db.ChucVus.Where(m => m.Status == 0).ToList();
            foreach (var chucVu in deletedChucVus)
            {
                chucVu.Status = 1;
            }
            db.SaveChanges();
            TempData["SuccessMessage"] = "Tất cả chức vụ đã được khôi phục!";
            return RedirectToAction("Index");
        }

        // Xóa tất cả chức vụ trong thùng rác vĩnh viễn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAllForever()
        {
            var deletedChucVus = db.ChucVus.Where(m => m.Status == 0).ToList();
            db.ChucVus.RemoveRange(deletedChucVus);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Tất cả chức vụ đã bị xóa vĩnh viễn!";
            return RedirectToAction("Index");
        }

        // Giải phóng bộ nhớ
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