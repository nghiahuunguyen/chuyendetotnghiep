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
    public class SanPhamsController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        public ActionResult Search(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return RedirectToAction("Index"); // Nếu không nhập gì, hiển thị tất cả
            }

            // Use Contains() instead of == for partial matching
            var sanphams = db.SanPhams.Where(h => h.TenSP.Contains(keyword) || h.TuKhoa.Contains(keyword)).ToList();

            if (sanphams == null || !sanphams.Any())
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm nào phù hợp.";
                return RedirectToAction("Index");
            }

            return View("Index", sanphams.ToPagedList(1, 5)); // Trả về danh sách các sản phẩm phù hợp
        }

        // GET: Admin/SanPhams
        public ActionResult Index(int status = 3, string keyword = "", int? page = 1)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }
            int pageSize = 5; // 5 sản phẩm mỗi trang
            int pageNumber = (page ?? 1);

            var sanPhams = db.SanPhams.AsQueryable();

           
            if (!string.IsNullOrEmpty(keyword))
            {
                sanPhams = sanPhams.Where(sp => sp.TenSP.Contains(keyword) || sp.TuKhoa.Contains(keyword));
            }

            switch (status)
            {
                case 1:
                    sanPhams = sanPhams.Where(sp => sp.Status == 1);
                    break;
                case 2:
                    sanPhams = sanPhams.Where(sp => sp.Status == 2);
                    break;
                case 0:
                    sanPhams = sanPhams.Where(sp => sp.Status == 0);
                    break;
                case 3: // Active + Unpublished
                    sanPhams = sanPhams.Where(sp => sp.Status == 1 || sp.Status == 2);
                    break;
            }

            // Sắp xếp sản phẩm trước khi phân trang
            sanPhams = sanPhams.OrderBy(s => s.MaSP);

            // Lưu các bộ lọc vào ViewBag để duy trì trạng thái khi chuyển trang
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentKeyword = keyword;

            return View(sanPhams.ToPagedList(pageNumber, pageSize));
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
                TempData["SuccessMessage"] = "Sản phẩm đã được chuyển sang trạng thái không xuất bản.";
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
        public ActionResult Create([Bind(Include = "MaLoaiSP,MaHang,TenSP,SoLuong,Link,KhuyenMai,TuKhoa,GiaNhap,GiaDau,SoGiam,MoTa,Status")] SanPham sanPham, HttpPostedFileBase HinhAnh)
        {
            if (ModelState.IsValid)
            {
                // Lấy mã sản phẩm lớn nhất
                var lastSP = db.SanPhams.OrderByDescending(sp => sp.MaSP).FirstOrDefault();
                int newId = 1;

                if (lastSP != null && lastSP.MaSP.StartsWith("SP"))
                {
                    int.TryParse(lastSP.MaSP.Substring(2), out newId);
                    newId++;
                }

                sanPham.MaSP = $"SP{newId:D3}";

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
        public ActionResult Edit([Bind(Include = "MaSP,MaLoaiSP,MaHang,TenSP,SoLuong,Link,KhuyenMai,TuKhoa,GiaNhap,GiaDau,SoGiam,MoTa")] SanPham sanPham, HttpPostedFileBase HinhAnh)
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

        public ActionResult Trash(int? page = 1)
        {
            int pageSize = 5; // 5 sản phẩm mỗi trang
            int pageNumber = (page ?? 1);

            var deletedSanPhams = db.SanPhams.Where(sp => sp.Status == 0).OrderBy(s => s.MaSP);
            return View(deletedSanPhams.ToPagedList(pageNumber, pageSize));
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
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForever(string id)
        {
            var sanPham = db.SanPhams.Find(id);
            if (sanPham == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm để xóa.";
                return RedirectToAction("Index", new { status = "Deleted" });
            }

            try
            {
                db.SanPhams.Remove(sanPham);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Sản phẩm đã bị xóa vĩnh viễn!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Không thể xóa vì sản phẩm đang được sử dụng trong dữ liệu khác. Vui lòng kiểm tra lại!";
            }

            return RedirectToAction("Index", new { status = "Deleted" });
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
            try
            {
                var deletedSanPhams = db.SanPhams.Where(sp => sp.Status == 0).ToList();
                db.SanPhams.RemoveRange(deletedSanPhams);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Tất cả sản phẩm đã bị xóa vĩnh viễn!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Không thể xóa vì một số sản phẩm đang được sử dụng trong dữ liệu khác. Vui lòng kiểm tra lại!";
            }

            return RedirectToAction("Index", new { status = "Deleted" });
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}