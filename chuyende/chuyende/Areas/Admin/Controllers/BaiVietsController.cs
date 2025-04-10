using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using chuyende.Models;

namespace chuyende.Areas.Admin.Controllers
{
    public class BaiVietsController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        public ActionResult Index(int status = 3, string keyword = "", int? page = 1)
        {
            if (Session["Admin"] == null)
                return RedirectToAction("Index", "DangNhap");

            int pageSize = 5;
            int pageNumber = page ?? 1;

            var baiViets = db.BaiViets.Include(b => b.LoaiBaiViet).AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                baiViets = baiViets.Where(b => b.TenBV.Contains(keyword) || b.Link.Contains(keyword));
            }

            switch (status)
            {
                case 1: baiViets = baiViets.Where(b => b.Status == 1); break;
                case 2: baiViets = baiViets.Where(b => b.Status == 2); break;
                case 0: baiViets = baiViets.Where(b => b.Status == 0); break;
                case 3: baiViets = baiViets.Where(b => b.Status == 1 || b.Status == 2); break;
            }

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentKeyword = keyword;

            return View(baiViets.OrderBy(b => b.MaBV).ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ToggleStatus(string id)
        {
            var baiviet = db.BaiViets.Find(id);
            if (baiviet == null)
            {
                return Json(new { success = false });
            }

            baiviet.Status = (baiviet.Status == 1) ? 2 : 1;
            db.SaveChanges();

            return Json(new { success = true, status = baiviet.Status });
        }

        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var baiViet = db.BaiViets.Include("LoaiBaiViet").FirstOrDefault(b => b.MaBV == id);
            if (baiViet == null)
            {
                return HttpNotFound();
            }

            return View(baiViet);
        }


        public ActionResult Create()
        {
            ViewBag.MaLoaiBV = new SelectList(db.LoaiBaiViets, "MaLoaiBV", "TenLoaiBV");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TenBV,NôiDung,Link,MaLoaiBV")] BaiViet baiViet, HttpPostedFileBase HinhAnh)
        {
            if (ModelState.IsValid)
            {
                var last = db.BaiViets.OrderByDescending(b => b.MaBV).FirstOrDefault();
                int newId = 1;
                if (last != null && last.MaBV.StartsWith("BV"))
                {
                    int.TryParse(last.MaBV.Substring(2), out newId);
                    newId++;
                }
                baiViet.MaBV = $"BV{newId:D3}";

                if (HinhAnh != null && HinhAnh.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(HinhAnh.FileName);
                    var path = Path.Combine(Server.MapPath("~/img/baiviet"), fileName);
                    HinhAnh.SaveAs(path);
                    baiViet.HinhAnh = fileName;
                }

                baiViet.Status = 1;
                db.BaiViets.Add(baiViet);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Thêm bài viết thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.MaLoaiBV = new SelectList(db.LoaiBaiViets, "MaLoaiBV", "TenLoaiBV", baiViet.MaLoaiBV);
            return View(baiViet);
        }

        public ActionResult Edit(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var baiViet = db.BaiViets.Find(id);
            if (baiViet == null) return HttpNotFound();

            ViewBag.MaLoaiBV = new SelectList(db.LoaiBaiViets, "MaLoaiBV", "TenLoaiBV", baiViet.MaLoaiBV);
            return View(baiViet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaBV,TenBV,NoiDung,Link,MaLoaiBV")] BaiViet baiViet, HttpPostedFileBase HinhAnh)
        {
            if (ModelState.IsValid)
            {
                var baiVietCu = db.BaiViets.Find(baiViet.MaBV);
                if (baiVietCu == null)
                {
                    return HttpNotFound();
                }

                // Cập nhật các trường từ form
                baiVietCu.TenBV = baiViet.TenBV;
                baiVietCu.NoiDung = baiViet.NoiDung;
                baiVietCu.Link = baiViet.Link;
                baiVietCu.MaLoaiBV = baiViet.MaLoaiBV;
                baiVietCu.Status = 1;

                // Nếu có hình mới thì cập nhật
                if (HinhAnh != null && HinhAnh.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(HinhAnh.FileName);
                    var path = Path.Combine(Server.MapPath("~/img/baiviet"), fileName);
                    HinhAnh.SaveAs(path);
                    baiVietCu.HinhAnh = fileName;
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật bài viết thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.MaLoaiBV = new SelectList(db.LoaiBaiViets, "MaLoaiBV", "TenLoaiBV", baiViet.MaLoaiBV);
            return View(baiViet);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoveToTrash(string id)
        {
            var bv = db.BaiViets.Find(id);
            if (bv != null)
            {
                bv.Status = 0;
                db.SaveChanges();
                TempData["WarningMessage"] = "Đã chuyển bài viết vào thùng rác.";
            }
            return RedirectToAction("Index");
        }

        public ActionResult Trash(int? page = 1)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;
            var trash = db.BaiViets.Where(b => b.Status == 0).OrderBy(b => b.MaBV);
            return View(trash.ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Restore(string id)
        {
            var bv = db.BaiViets.Find(id);
            if (bv != null)
            {
                bv.Status = 1;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã khôi phục bài viết.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForever(string id)
        {
            var bv = db.BaiViets.Find(id);
            if (bv == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài viết.";
                return RedirectToAction("Index");
            }
            try
            {
                db.BaiViets.Remove(bv);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã xóa vĩnh viễn.";
            }
            catch
            {
                TempData["ErrorMessage"] = "Không thể xóa bài viết này.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RestoreAll()
        {
            var all = db.BaiViets.Where(b => b.Status == 0).ToList();
            foreach (var bv in all) bv.Status = 1;
            db.SaveChanges();
            TempData["SuccessMessage"] = "Đã khôi phục tất cả bài viết.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAllForever()
        {
            try
            {
                var all = db.BaiViets.Where(b => b.Status == 0).ToList();
                db.BaiViets.RemoveRange(all);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã xóa vĩnh viễn tất cả.";
            }
            catch
            {
                TempData["ErrorMessage"] = "Không thể xóa hết bài viết.";
            }
            return RedirectToAction("Trash");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
