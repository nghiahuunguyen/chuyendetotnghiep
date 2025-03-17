using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using chuyende.Models;

namespace chuyende.Areas.Admin.Controllers
{
    public class NhanViensController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        public ActionResult Index(string status = "Active")
        {
            var nhanViens = db.NhanViens.AsQueryable();
            if (status == "Active")
                nhanViens = nhanViens.Where(nv => nv.Status == 1);
            else if (status == "Deleted")
                nhanViens = nhanViens.Where(nv => nv.Status == 0);

            return View(nhanViens.ToList());
        }

        public ActionResult Details(string id)
        {
            if (id == null) return RedirectToAction("Index");
            NhanVien nhanVien = db.NhanViens.Find(id);
            if (nhanVien == null) return RedirectToAction("Index");
            return View(nhanVien);
        }

        public ActionResult Create()
        {
            ViewBag.MaCV = new SelectList(db.ChucVus.Where(cv => cv.Status == 1), "MaCV", "TenCV");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TenNV,SoDienThoai,Email,NgaySinh,GioiTinh,CCCD,DiaChi,TenDN,MatKhau,MaCV")] NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                // Lấy mã NV lớn nhất hiện tại, nếu không có thì bắt đầu từ NV001
                var lastNhanVien = db.NhanViens.OrderByDescending(nv => nv.MaNV).FirstOrDefault();
                int newId = (lastNhanVien != null && lastNhanVien.MaNV.StartsWith("NV"))
                    ? int.Parse(lastNhanVien.MaNV.Substring(2)) + 1
                    : 1;

                // Gán mã mới với format NV001, NV002, ...
                nhanVien.MaNV = $"NV{newId:D3}";
                nhanVien.Status = 1; // Đánh dấu nhân viên đang hoạt động

                db.NhanViens.Add(nhanVien);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.MaCV = new SelectList(db.ChucVus.Where(cv => cv.Status == 1), "MaCV", "TenCV", nhanVien.MaCV);
            return View(nhanVien);
        }


        public ActionResult Edit(string id)
        {
            if (id == null) return RedirectToAction("Index");
            NhanVien nhanVien = db.NhanViens.Find(id);
            if (nhanVien == null) return RedirectToAction("Index");
            ViewBag.MaCV = new SelectList(db.ChucVus, "MaCV", "TenCV", nhanVien.MaCV);
            return View(nhanVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaNV,TenNV,SoDienThoai,Email,NgaySinh,GioiTinh,CCCD,DiaChi,TenDN,MatKhau,MaCV")] NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                var existingNhanVien = db.NhanViens.Find(nhanVien.MaNV);
                if (existingNhanVien != null)
                {
                    existingNhanVien.TenNV = nhanVien.TenNV;
                    existingNhanVien.SoDienThoai = nhanVien.SoDienThoai;
                    existingNhanVien.Email = nhanVien.Email;
                    existingNhanVien.NgaySinh = nhanVien.NgaySinh;
                    existingNhanVien.GioiTinh = nhanVien.GioiTinh;
                    existingNhanVien.CCCD = nhanVien.CCCD;
                    existingNhanVien.DiaChi = nhanVien.DiaChi;
                    existingNhanVien.TenDN = nhanVien.TenDN;
                    existingNhanVien.MatKhau = nhanVien.MatKhau;
                    existingNhanVien.MaCV = nhanVien.MaCV;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật nhân viên thành công!";
                }
                return RedirectToAction("Index");
            }
            return View(nhanVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoveToTrash(string id)
        {
            NhanVien nhanVien = db.NhanViens.Find(id);
            if (nhanVien != null)
            {
                nhanVien.Status = 0;
                db.SaveChanges();
                TempData["WarningMessage"] = "Nhân viên đã được chuyển vào thùng rác.";
            }
            return RedirectToAction("Index");
        }

        public ActionResult Trash()
        {
            var deletedNhanViens = db.NhanViens.Where(nv => nv.Status == 0).ToList();
            return View(deletedNhanViens);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Restore(string id)
        {
            NhanVien nhanVien = db.NhanViens.Find(id);
            if (nhanVien != null)
            {
                nhanVien.Status = 1;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Nhân viên đã được khôi phục!";
            }
            return RedirectToAction("Trash");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForever(string id)
        {
            NhanVien nhanVien = db.NhanViens.Find(id);
            if (nhanVien != null)
            {
                db.NhanViens.Remove(nhanVien);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Nhân viên đã bị xóa vĩnh viễn.";
            }
            return RedirectToAction("Trash");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RestoreAll()
        {
            var deletedNhanViens = db.NhanViens.Where(nv => nv.Status == 0).ToList();
            foreach (var nhanVien in deletedNhanViens)
            {
                nhanVien.Status = 1; // Đặt lại trạng thái hoạt động
            }
            db.SaveChanges();
            TempData["SuccessMessage"] = "Tất cả nhân viên đã được khôi phục!";
            return RedirectToAction("Index"); // Chuyển hướng về danh sách nhân viên
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAllForever()
        {
            var deletedNhanViens = db.NhanViens.Where(nv => nv.Status == 0).ToList();
            if (deletedNhanViens.Any())
            {
                db.NhanViens.RemoveRange(deletedNhanViens); // Xóa tất cả nhân viên trong thùng rác
                db.SaveChanges();
                TempData["SuccessMessage"] = "Tất cả nhân viên đã bị xóa vĩnh viễn.";
            }
            return RedirectToAction("Index"); // Chuyển hướng về danh sách nhân viên
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
