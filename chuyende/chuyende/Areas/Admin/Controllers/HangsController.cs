using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using chuyende.Models;
using PagedList;
using PagedList.Mvc;

namespace chuyende.Areas.Admin.Controllers
{
    public class HangsController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();


        public ActionResult Index(string status = "Active", string keyword = "", int? page = 1)
        {
            int pageSize = 5; // Hiển thị 5 hãng mỗi trang
            int pageNumber = (page ?? 1); // Nếu không có số trang, mặc định là trang 1

            var hangs = db.Hangs.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                hangs = hangs.Where(h => h.TenHang.Contains(keyword) || h.TuKhoa.Contains(keyword));
            }

            switch (status)
            {
                case "Active":
                    hangs = hangs.Where(h => h.Status == 1);
                    break;
                case "Unpublished":
                    hangs = hangs.Where(h => h.Status == 2);
                    break;
                case "Deleted":
                    hangs = hangs.Where(h => h.Status == 0);
                    break;
            }

            return View(hangs.OrderBy(h => h.TenHang).ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Publish(string id)
        {
            var hang = db.Hangs.Find(id);
            if (hang != null)
            {
                hang.Status = 1; // Đánh dấu là Xuất bản
                db.SaveChanges();
                TempData["SuccessMessage"] = "Hãng đã được xuất bản!";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Unpublish(string id)
        {
            var hang = db.Hangs.Find(id);
            if (hang != null)
            {
                hang.Status = 2; // Đánh dấu là Không xuất bản
                db.SaveChanges();
                TempData["WarningMessage"] = "Hãng đã được chuyển sang trạng thái không xuất bản.";
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult ToggleStatus(string id)
        {
            var hang = db.Hangs.Find(id);
            if (hang != null)
            {
                hang.Status = (hang.Status == 1) ? 2 : 1; // Chuyển đổi trạng thái
                db.SaveChanges();
                return Json(new { success = true, status = hang.Status });
            }
            return Json(new { success = false });
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

        // GET: Admin/Hangs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Hangs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaHang,TenHang,Logo,SoDienThoai,Email,DiaChi,TuKhoa,Status")] Hang hang, HttpPostedFileBase Logo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy mã hãng lớn nhất từ database
                    var maxHang = db.Hangs
                        .Where(h => h.MaHang.StartsWith("Hang") && h.MaHang.Length == 7)
                        .OrderByDescending(h => h.MaHang)
                        .Select(h => h.MaHang)
                        .FirstOrDefault();

                    // Xác định số ID mới
                    int newId = (maxHang != null && int.TryParse(maxHang.Substring(4), out int id)) ? id + 1 : 1;
                    hang.MaHang = $"Hang{newId:D3}"; // Định dạng Hang001, Hang002, ...

                    // Xử lý upload file Logo
                    if (Logo != null && Logo.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(Logo.FileName);
                        var path = Path.Combine(Server.MapPath("~/img/hang"), fileName);
                        Logo.SaveAs(path);
                        hang.Logo = fileName;
                    }

                    // Gán trạng thái mặc định
                    hang.Status = 1;
                    db.Hangs.Add(hang);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Thêm hãng thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm hãng! " + ex.Message;
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

        public ActionResult Trash()
        {
            var deletedHangs = db.Hangs.Where(h => h.Status == 0);

           

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
