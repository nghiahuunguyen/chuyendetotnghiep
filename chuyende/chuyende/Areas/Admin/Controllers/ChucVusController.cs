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
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }
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
            TempData["ErrorMessage"] = "Thêm chức vụ thất bại. Vui lòng kiểm tra lại dữ liệu!";
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
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy chức vụ để cập nhật!";
                }
                return RedirectToAction("Index");
            }
            TempData["ErrorMessage"] = "Cập nhật chức vụ thất bại. Vui lòng kiểm tra lại dữ liệu!";
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
                TempData["SuccessMessage"] = "Chức vụ đã được chuyển vào thùng rác.";
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
            return RedirectToAction("Index");
        }

        // Xóa vĩnh viễn một chức vụ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForever(string id)
        {
            var chucVu = db.ChucVus.Find(id);
            if (chucVu != null)
            {
                bool isReferenced = db.NhanViens.Any(nv => nv.MaCV == id);
                if (isReferenced)
                {
                    TempData["ErrorMessage"] = "Không thể xóa chức vụ vì đang được sử dụng bởi nhân viên!";
                }
                else
                {
                    db.ChucVus.Remove(chucVu);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Chức vụ đã bị xóa vĩnh viễn!";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy chức vụ để xóa!";
            }
            return RedirectToAction("Index");
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
            // Lấy các chức vụ có Status = 0
            var deletedChucVus = db.ChucVus.Where(m => m.Status == 0).ToList();

            // Lọc ra các chức vụ không bị liên kết bởi NhanVien
            var chucVusToDelete = deletedChucVus
                .Where(cv => !db.NhanViens.Any(nv => nv.MaCV == cv.MaCV))
                .ToList();

            // Nếu có chức vụ hợp lệ để xóa
            if (chucVusToDelete.Any())
            {
                db.ChucVus.RemoveRange(chucVusToDelete);
                db.SaveChanges();
                TempData["SuccessMessage"] = $"{chucVusToDelete.Count} chức vụ đã bị xóa vĩnh viễn!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không có chức vụ nào đủ điều kiện để xóa (vì đang được sử dụng)!";
            }

            return RedirectToAction("Index", new { status = "Deleted" });
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