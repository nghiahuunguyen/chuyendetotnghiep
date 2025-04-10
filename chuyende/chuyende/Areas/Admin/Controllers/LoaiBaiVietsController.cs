using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using chuyende.Models;
using PagedList;

namespace chuyende.Areas.Admin.Controllers
{
    public class LoaiBaiVietsController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        // GET: Admin/LoaiBaiViets
        public ActionResult Index(int status = 3, string keyword = "", int page = 1)
        {
            if (Session["Admin"] == null)
                return RedirectToAction("Index", "DangNhap");

            int pageSize = 5;
            var loaiBaiViets = db.LoaiBaiViets.AsQueryable();

            switch (status)
            {
                case 1:
                    loaiBaiViets = loaiBaiViets.Where(x => x.Status == 1); break;
                case 2:
                    loaiBaiViets = loaiBaiViets.Where(x => x.Status == 2); break;
                case 0:
                    loaiBaiViets = loaiBaiViets.Where(x => x.Status == 0); break;
                default:
                    loaiBaiViets = loaiBaiViets.Where(x => x.Status != 0); break;
            }

            if (!string.IsNullOrEmpty(keyword))
                loaiBaiViets = loaiBaiViets.Where(x => x.TenLoaiBV.Contains(keyword));

            var pagedList = loaiBaiViets.OrderBy(x => x.MaLoaiBV).ToPagedList(page, pageSize);

            ViewBag.Status = status;
            ViewBag.Keyword = keyword;

            return View(pagedList);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ToggleStatus(string id)
        {
            var loai = db.LoaiBaiViets.Find(id);
            if (loai == null)
            {
                return Json(new { success = false });
            }

            loai.Status = (loai.Status == 1) ? 2 : 1;
            db.SaveChanges();

            return Json(new { success = true, status = loai.Status });
        }

        // Hiển thị chi tiết loại bài viết
        public ActionResult Details(string id)
        {
            if (id == null) return RedirectToAction("Index");

            LoaiBaiViet loaiBaiViet = db.LoaiBaiViets.Find(id);

            if (loaiBaiViet == null || loaiBaiViet.Status == 0)
                return RedirectToAction("Index");

            return View(loaiBaiViet);
        }

        // GET: Admin/LoaiBaiViets/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/LoaiBaiViets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TenLoaiBV")] LoaiBaiViet loai)
        {
            if (ModelState.IsValid)
            {
                var maxId = db.LoaiBaiViets.OrderByDescending(l => l.MaLoaiBV).Select(l => l.MaLoaiBV).FirstOrDefault();
                int nextId = (maxId != null) ? int.Parse(maxId.Substring(3)) + 1 : 1;
                loai.MaLoaiBV = "LBV" + nextId.ToString("D3");
                loai.Status = 1;
                db.LoaiBaiViets.Add(loai);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Thêm loại bài viết thành công!";
                return RedirectToAction("Index");
            }

            return View(loai);
        }

        // GET: Admin/LoaiBaiViets/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var loai = db.LoaiBaiViets.Find(id);
            if (loai == null)
                return HttpNotFound();

            return View(loai);
        }

        // POST: Admin/LoaiBaiViets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaLoaiBV,TenLoaiBV")] LoaiBaiViet loai)
        {
            if (ModelState.IsValid)
            {
                var existing = db.LoaiBaiViets.Find(loai.MaLoaiBV);
                if (existing != null)
                {
                    existing.TenLoaiBV = loai.TenLoaiBV;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật thành công!";
                    return RedirectToAction("Index");
                }
            }

            return View(loai);
        }

        // POST: Admin/LoaiBaiViets/MoveToTrash/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoveToTrash(string id)
        {
            var loai = db.LoaiBaiViets.Find(id);
            if (loai != null)
            {
                loai.Status = 0;
                db.SaveChanges();
                TempData["ErrorMessage"] = "Đã chuyển vào thùng rác.";
            }
            return RedirectToAction("Index");
        }

        // GET: Admin/LoaiBaiViets/Trash
        public ActionResult Trash()
        {
            var deletedLoaiBaiViets = db.LoaiBaiViets.Where(lbv => lbv.Status == 0).ToList();
            return View(deletedLoaiBaiViets);
        }

        // POST: Admin/LoaiBaiViets/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Restore(string id)
        {
            var loai = db.LoaiBaiViets.Find(id);
            if (loai != null)
            {
                loai.Status = 1;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Khôi phục thành công.";
            }
            return RedirectToAction("Index");
        }

        // POST: Admin/LoaiBaiViets/RestoreAll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RestoreAll()
        {
            var list = db.LoaiBaiViets.Where(x => x.Status == 0).ToList();
            foreach (var item in list)
            {
                item.Status = 1;
            }
            db.SaveChanges();
            TempData["SuccessMessage"] = "Khôi phục tất cả thành công.";
            return RedirectToAction("Index");
        }

        // POST: Admin/LoaiBaiViets/DeleteForever/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForever(string id)
        {
            var loai = db.LoaiBaiViets.Find(id);
            if (loai != null)
            {
                db.LoaiBaiViets.Remove(loai);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã xóa vĩnh viễn.";
            }
            return RedirectToAction("Index");
        }

        // POST: Admin/LoaiBaiViets/DeleteAllForever
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAllForever()
        {
            var trashList = db.LoaiBaiViets.Where(x => x.Status == 0).ToList();
            db.LoaiBaiViets.RemoveRange(trashList);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Đã xóa tất cả vĩnh viễn.";
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
