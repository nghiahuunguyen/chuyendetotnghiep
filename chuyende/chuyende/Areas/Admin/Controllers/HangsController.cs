using System;
using System.Collections.Generic;
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

        public ActionResult Index(string status = "Active", string keyword = "")
        {
            var hangs = db.Hangs.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                hangs = hangs.Where(h => h.TenHang.Contains(keyword) || h.TuKhoa.Contains(keyword));
            }

            if (status == "Active")
            {
                hangs = hangs.Where(h => h.Status == 1);
            }
            else if (status == "Deleted")
            {
                hangs = hangs.Where(h => h.Status == 0);
            }
            return View(hangs.ToList());
        }

        public ActionResult Details(string id)
        {
            if (id == null)
                return RedirectToAction("Index");
            Hang hang = db.Hangs.Find(id);
            if (hang == null)
                return RedirectToAction("Index");
            return View(hang);
        }

        public ActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TenHang,SoDienThoai,Email,DiaChi,TuKhoa,Status")] Hang hang, HttpPostedFileBase Logo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy mã NV lớn nhất hiện tại, nếu không có thì bắt đầu từ NV001
                    var lastHang = db.Hangs.OrderByDescending(h => h.MaHang).FirstOrDefault();
                    int newId = (lastHang != null && lastHang.MaHang.StartsWith("Hang"))
                        ? int.Parse(lastHang.MaHang.Substring(2)) + 1
                        : 1;

                    // Gán mã mới với format NV001, NV002, ...
                    hang.MaHang = $"Hang{newId:D3}";
                    // Kiểm tra và lưu ảnh nếu có
                    if (Logo != null && Logo.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(Logo.FileName);
                        var path = Path.Combine(Server.MapPath("~/img/hang"), fileName);
                        Logo.SaveAs(path);
                        hang.Logo = fileName;
                    }

                    hang.Status = 1; // Đánh dấu hãng là Active
                    db.Hangs.Add(hang);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Thêm hãng thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }
            return View(hang);
        }

        public ActionResult Edit(string id)
        {
            if (id == null)
                return RedirectToAction("Index");
            Hang hang = db.Hangs.Find(id);
            if (hang == null)
                return RedirectToAction("Index");
            return View(hang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaHang,TenHang,Logo,SoDienThoai,Email,DiaChi,TuKhoa")] Hang hang, HttpPostedFileBase LogoFile)
        {
            if (ModelState.IsValid)
            {
                var existingHang = db.Hangs.Find(hang.MaHang);
                if (existingHang != null)
                {
                    existingHang.TenHang = hang.TenHang;
                    existingHang.SoDienThoai = hang.SoDienThoai;
                    existingHang.Email = hang.Email;
                    existingHang.DiaChi = hang.DiaChi;
                    existingHang.TuKhoa = hang.TuKhoa;

                    if (LogoFile != null && LogoFile.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(LogoFile.FileName);
                        var path = Path.Combine(Server.MapPath("~/img/hang"), fileName);
                        LogoFile.SaveAs(path);
                        existingHang.Logo = fileName;
                    }
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật hãng thành công!";
                }
                return RedirectToAction("Index");
            }
            return View(hang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoveToTrash(string id)
        {
            Hang hang = db.Hangs.Find(id);
            if (hang != null)
            {
                hang.Status = 0;
                db.SaveChanges();
                TempData["WarningMessage"] = "Hãng đã được chuyển vào thùng rác.";
            }
            return RedirectToAction("Index");
        }

        public ActionResult Trash(string keyword = "")
        {
            var deletedHangs = db.Hangs.Where(h => h.Status == 0);

            if (!string.IsNullOrEmpty(keyword))
            {
                deletedHangs = deletedHangs.Where(h => h.TenHang.Contains(keyword) || h.TuKhoa.Contains(keyword));
            }

            return View(deletedHangs.ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Restore(string id)
        {
            Hang hang = db.Hangs.Find(id);
            if (hang != null)
            {
                hang.Status = 1;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Hãng đã được khôi phục!";
            }
            return RedirectToAction("Trash");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForever(string id)
        {
            Hang hang = db.Hangs.Find(id);
            if (hang != null)
            {
                db.Hangs.Remove(hang);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Hãng đã bị xóa vĩnh viễn!";
            }
            return RedirectToAction("Trash");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RestoreAll()
        {
            var deletedHangs = db.Hangs.Where(h => h.Status == 0).ToList();
            foreach (var hang in deletedHangs)
            {
                hang.Status = 1;
            }
            db.SaveChanges();
            TempData["SuccessMessage"] = "Tất cả hãng đã được khôi phục!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAllForever()
        {
            var deletedHangs = db.Hangs.Where(h => h.Status == 0).ToList();
            db.Hangs.RemoveRange(deletedHangs);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Tất cả hãng đã bị xóa vĩnh viễn!";
            return RedirectToAction("Index");
        }
    }
}
