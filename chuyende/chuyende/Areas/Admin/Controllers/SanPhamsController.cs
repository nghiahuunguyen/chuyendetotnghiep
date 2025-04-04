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
using PagedList;

namespace chuyende.Areas.Admin.Controllers
{
    public class SanPhamsController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        public ActionResult Search(string keyword = "", int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return RedirectToAction("Index");
            }

            var sanphams = db.SanPhams
                .Where(sp => sp.TenSP.Contains(keyword) || sp.TuKhoa.Contains(keyword))
                .OrderBy(sp => sp.MaSP)
                .ToPagedList(page, pageSize);

            ViewBag.Keyword = keyword;
            return View(sanphams);
        }



        // GET: Admin/SanPhams
        public ActionResult Index(string status = "Active", string keyword = "", int page = 1, int pageSize = 10)
        {
            var sanPhams = db.SanPhams.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                sanPhams = sanPhams.Where(sp => sp.TenSP.Contains(keyword) || sp.TuKhoa.Contains(keyword));
                ViewBag.Keyword = keyword;
            }

            switch (status)
            {
                case "Active":
                    sanPhams = sanPhams.Where(sp => sp.Status == 1);
                    break;
                case "Unpublished":
                    sanPhams = sanPhams.Where(sp => sp.Status == 2);
                    break;
                case "Deleted":
                    sanPhams = sanPhams.Where(sp => sp.Status == 0);
                    break;
            }

            sanPhams = sanPhams.OrderBy(sp => sp.MaSP);

            return View(sanPhams.ToPagedList(page, pageSize));
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Publish(string id)
        {
            var sanPham = db.SanPhams.Find(id);
            if (sanPham != null)
            {
                sanPham.Status = 1; // Xuất bản sản phẩm
                db.SaveChanges();
                TempData["SuccessMessage"] = "Sản phẩm đã được xuất bản!";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Unpublish(string id)
        {
            var sanPham = db.SanPhams.Find(id);
            if (sanPham != null)
            {
                sanPham.Status = 2; // Không xuất bản
                db.SaveChanges();
                TempData["WarningMessage"] = "Sản phẩm đã được chuyển sang trạng thái không xuất bản.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ToggleStatus(string id)
        {
            var sanPham = db.SanPhams.Find(id);
            if (sanPham != null)
            {
                sanPham.Status = (sanPham.Status == 1) ? 2 : 1; // Chuyển đổi trạng thái
                db.SaveChanges();
                return Json(new { success = true, status = sanPham.Status });
            }
            return Json(new { success = false });
        }

        public ActionResult Details(string id)
        {
            if (id == null) return RedirectToAction("Index");
            SanPham sanPham = db.SanPhams.Find(id);
            if (sanPham == null) return RedirectToAction("Index");
            return View(sanPham);
        }

        public ActionResult Create()
        {
            ViewBag.MaHang = new SelectList(db.Hangs, "MaHang", "TenHang");
            ViewBag.MaLoaiSP = new SelectList(db.LoaiSanPhams, "MaLoaiSP", "TenLoaiSP");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaLoaiSP,MaHang,TenSP,SoLuong,KhuyenMai,TuKhoa,GiaNhap,GiaDau,SoGiam,MoTa,Status")] SanPham sanPham, HttpPostedFileBase HinhAnh)
        {
            if (ModelState.IsValid)
            {
                // Lấy mã NV lớn nhất hiện tại, nếu không có thì bắt đầu từ NV001
                var lastSP = db.SanPhams
                                        .Where(sp => sp.MaSP.StartsWith("SP"))
                                        .OrderByDescending(sp => sp.MaSP)
                                        .Select(sp => sp.MaSP)
                                        .FirstOrDefault();

                int newId = 1; // Mặc định nếu không có sản phẩm nào

                if (lastSP != null)
                {
                    string numberPart = lastSP.Substring(2); // Lấy phần số từ "SP001"
                    if (int.TryParse(numberPart, out int idNumber))
                    {
                        newId = idNumber + 1;
                    }
                }

                sanPham.MaSP = $"SP{newId:D3}"; // Format thành SP001, SP002...


                if (HinhAnh != null && HinhAnh.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(HinhAnh.FileName);
                    var path = Path.Combine(Server.MapPath("~/img/sanpham"), fileName);
                    HinhAnh.SaveAs(path);
                    sanPham.HinhAnh = fileName;
                }

                sanPham.Status = 1;
                db.SanPhams.Add(sanPham);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.MaHang = new SelectList(db.Hangs, "MaHang", "TenHang", sanPham.MaHang);
            ViewBag.MaLoaiSP = new SelectList(db.LoaiSanPhams, "MaLoaiSP", "TenLoaiSP", sanPham.MaLoaiSP);
            return View(sanPham);
        }


        // GET: Admin/SanPhams/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            SanPham sanPham = db.SanPhams.Find(id);
            if (sanPham == null)
                return HttpNotFound();

            ViewBag.MaHang = new SelectList(db.Hangs, "MaHang", "TenHang", sanPham.MaHang);
            ViewBag.MaLoaiSP = new SelectList(db.LoaiSanPhams, "MaLoaiSP", "TenLoaiSP", sanPham.MaLoaiSP);
            return View(sanPham);
        }

        // POST: Admin/SanPhams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaSP,MaLoaiSP,MaHang,TenSP,SoLuong,KhuyenMai,TuKhoa,GiaNhap,GiaDau,SoGiam,MoTa")] SanPham sanPham, HttpPostedFileBase HinhAnh)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (HinhAnh != null && HinhAnh.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(HinhAnh.FileName);
                        var path = Path.Combine(Server.MapPath("~/img/sanpham"), fileName);
                        HinhAnh.SaveAs(path);
                        sanPham.HinhAnh = fileName;
                    }
                    sanPham.Status = 1;
                    db.Entry(sanPham).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật sản phẩm!";
                }
            }
            ViewBag.MaHang = new SelectList(db.Hangs, "MaHang", "TenHang", sanPham.MaHang);
            ViewBag.MaLoaiSP = new SelectList(db.LoaiSanPhams, "MaLoaiSP", "TenLoaiSP", sanPham.MaLoaiSP);
            return View(sanPham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoveToTrash(string id)
        {
            var sanPham = db.SanPhams.Find(id);
            if (sanPham != null)
            {
                sanPham.Status = 0;
                db.SaveChanges();
                TempData["WarningMessage"] = "Sản phẩm đã được chuyển vào thùng rác.";
            }
            return RedirectToAction("Index");
        }

        public ActionResult Trash()
        {
            var deletedSanPhams = db.SanPhams.Where(sp => sp.Status == 0).ToList();
            return View(deletedSanPhams);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Restore(string id)
        {
            var sanPham = db.SanPhams.Find(id);
            if (sanPham != null)
            {
                sanPham.Status = 1;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Sản phẩm đã được khôi phục!";
            }
            return RedirectToAction("Trash");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForever(string id)
        {
            var sanPham = db.SanPhams.Find(id);
            if (sanPham != null)
            {
                db.SanPhams.Remove(sanPham);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Sản phẩm đã bị xóa vĩnh viễn!";
            }
            return RedirectToAction("Trash");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RestoreAll()
        {
            var deletedSanPhams = db.SanPhams.Where(sp => sp.Status == 0).ToList();
            foreach (var sanPham in deletedSanPhams)
            {
                sanPham.Status = 1;
            }
            db.SaveChanges();
            TempData["SuccessMessage"] = "Tất cả sản phẩm đã được khôi phục!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAllForever()
        {
            var deletedSanPhams = db.SanPhams.Where(sp => sp.Status == 0).ToList();
            db.SanPhams.RemoveRange(deletedSanPhams);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Tất cả sản phẩm đã bị xóa vĩnh viễn!";
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