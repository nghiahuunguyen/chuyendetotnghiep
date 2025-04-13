using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using chuyende.Models;

namespace chuyende.Controllers
{
    public class ModuleController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        // Hiển thị menu loại sản phẩm
        public ActionResult Menu()
        {
            var loaiSanPhams = db.LoaiSanPhams
                                 .Where(lsp => lsp.Status == 1)
                                 .ToList();
            return View(loaiSanPhams);
        }

        // Lọc sản phẩm theo loại
        public ActionResult ByLoai(string alias, decimal? giaTu, decimal? giaDen)
        {
            if (string.IsNullOrEmpty(alias))
                return HttpNotFound("Alias không hợp lệ.");

            var loai = db.LoaiSanPhams.FirstOrDefault(l => l.Link.Trim().ToLower() == alias.Trim().ToLower() && l.Status == 1);
            if (loai == null)
                return HttpNotFound("Không tìm thấy loại sản phẩm.");

            var sanPhamsQuery = db.SanPhams
                                  .Where(sp => sp.MaLoaiSP == loai.MaLoaiSP && sp.Status == 1 && sp.Hang.Status == 1);

            System.Diagnostics.Debug.WriteLine($"Total products before any processing: {sanPhamsQuery.Count()}");

            var sanPhams = sanPhamsQuery
                .ToList()
                .Select(sp => new
                {
                    SanPham = sp,
                    GiaBan = (sp.GiaDau.HasValue && sp.SoGiam.HasValue && sp.GiaDau > 0)
                             ? sp.GiaDau.Value - (sp.GiaDau.Value * sp.SoGiam.Value / 100)
                             : -1
                })
                .Where(sp => sp.GiaBan >= 0);

            System.Diagnostics.Debug.WriteLine($"giaTu: {giaTu}, giaDen: {giaDen}");
            System.Diagnostics.Debug.WriteLine($"Total products after removing invalid prices: {sanPhams.Count()}");

            // Lọc theo giá
            if (giaTu.HasValue && giaTu.Value >= 0)
            {
                sanPhams = sanPhams.Where(sp => sp.GiaBan >= giaTu.Value);
                System.Diagnostics.Debug.WriteLine($"Total products after filtering giaTu ({giaTu}): {sanPhams.Count()}");
            }
            if (giaDen.HasValue && giaDen.Value >= 0)
            {
                sanPhams = sanPhams.Where(sp => sp.GiaBan <= giaDen.Value);
                System.Diagnostics.Debug.WriteLine($"Total products after filtering giaDen ({giaDen}): {sanPhams.Count()}");
            }

            var finalSanPhams = sanPhams.Select(sp => sp.SanPham).ToList();

            // Debug danh sách sản phẩm cuối cùng
            foreach (var sp in finalSanPhams)
            {
                var giaBan = sp.GiaDau.HasValue && sp.SoGiam.HasValue
                             ? sp.GiaDau.Value - (sp.GiaDau.Value * sp.SoGiam.Value / 100)
                             : -1;
                System.Diagnostics.Debug.WriteLine($"Product: {sp.TenSP}, GiaBan: {giaBan}");
            }

            ViewBag.TenLoaiSP = loai.TenLoaiSP;
            ViewBag.Title = loai.TenLoaiSP;
            ViewBag.LoaiAlias = loai.Link;
            ViewBag.HangSanPhams = finalSanPhams.Select(sp => sp.Hang).Distinct().ToList();
            ViewBag.GiaTu = giaTu ?? 300000;
            ViewBag.GiaDen = giaDen ?? 50000000;

            if (!finalSanPhams.Any())
            {
                ViewBag.Message = "Không tìm thấy sản phẩm trong khoảng giá này.";
            }

            return View("ByLoai", finalSanPhams);
        }

        public ActionResult ByHang(string loaiAlias, string hangAlias, decimal? giaTu, decimal? giaDen)
        {
            if (string.IsNullOrEmpty(loaiAlias) || string.IsNullOrEmpty(hangAlias))
                return HttpNotFound("Alias không hợp lệ.");

            var loai = db.LoaiSanPhams.FirstOrDefault(l => l.Link.Trim().ToLower() == loaiAlias.Trim().ToLower() && l.Status == 1);
            if (loai == null)
                return HttpNotFound("Không tìm thấy loại sản phẩm.");

            var hang = db.Hangs.FirstOrDefault(h => h.Link.Trim().ToLower() == hangAlias.Trim().ToLower() && h.Status == 1);
            if (hang == null)
                return HttpNotFound("Không tìm thấy hãng sản phẩm.");

            var sanPhamsQuery = db.SanPhams
                                  .Where(sp => sp.MaLoaiSP == loai.MaLoaiSP && sp.MaHang == hang.MaHang && sp.Status == 1);

            System.Diagnostics.Debug.WriteLine($"Total products before any processing: {sanPhamsQuery.Count()}");

            var sanPhams = sanPhamsQuery
                .ToList()
                .Select(sp => new
                {
                    SanPham = sp,
                    GiaBan = (sp.GiaDau.HasValue && sp.SoGiam.HasValue && sp.GiaDau > 0)
                             ? sp.GiaDau.Value - (sp.GiaDau.Value * sp.SoGiam.Value / 100)
                             : -1
                })
                .Where(sp => sp.GiaBan >= 0);

            System.Diagnostics.Debug.WriteLine($"giaTu: {giaTu}, giaDen: {giaDen}");
            System.Diagnostics.Debug.WriteLine($"Total products after removing invalid prices: {sanPhams.Count()}");

            if (giaTu.HasValue && giaTu.Value >= 0)
            {
                sanPhams = sanPhams.Where(sp => sp.GiaBan >= giaTu.Value);
                System.Diagnostics.Debug.WriteLine($"Total products after filtering giaTu ({giaTu}): {sanPhams.Count()}");
            }
            if (giaDen.HasValue && giaDen.Value >= 0)
            {
                sanPhams = sanPhams.Where(sp => sp.GiaBan <= giaDen.Value);
                System.Diagnostics.Debug.WriteLine($"Total products after filtering giaDen ({giaDen}): {sanPhams.Count()}");
            }

            var finalSanPhams = sanPhams.Select(sp => sp.SanPham).ToList();

            foreach (var sp in finalSanPhams)
            {
                var giaBan = sp.GiaDau.HasValue && sp.SoGiam.HasValue
                             ? sp.GiaDau.Value - (sp.GiaDau.Value * sp.SoGiam.Value / 100)
                             : -1;
                System.Diagnostics.Debug.WriteLine($"Product: {sp.TenSP}, GiaBan: {giaBan}");
            }

            ViewBag.TenLoaiSP = loai.TenLoaiSP;
            ViewBag.Title = hang.TenHang;
            ViewBag.LoaiAlias = loai.Link;
            ViewBag.HangAlias = hang.Link;
            ViewBag.HangSanPhams = db.Hangs
                                     .Where(h => h.Status == 1 && h.SanPhams.Any(sp => sp.MaLoaiSP == loai.MaLoaiSP))
                                     .ToList();
            ViewBag.GiaTu = giaTu ?? 300000;
            ViewBag.GiaDen = giaDen ?? 50000000;

            if (!finalSanPhams.Any())
            {
                ViewBag.Message = "Không tìm thấy sản phẩm trong khoảng giá này.";
            }

            return View("ByHang", finalSanPhams);
        }


        public ActionResult ChiTiet(string loaiAlias, string alias, string version = null)
        {
            if (string.IsNullOrEmpty(alias))
            {
                return HttpNotFound("Alias không hợp lệ.");
            }

            // Kiểm tra loại sản phẩm có status = 1
            var loai = db.LoaiSanPhams
                         .FirstOrDefault(l => l.Link.Trim().ToLower() == loaiAlias.Trim().ToLower() && l.Status == 1);

            if (loai == null)
            {
                return HttpNotFound("Không tìm thấy loại sản phẩm.");
            }

            // Tìm sản phẩm theo alias trong loại sản phẩm và kiểm tra sản phẩm có status = 1
            var sanPham = db.SanPhams
                            .FirstOrDefault(sp => sp.Link.Trim().ToLower() == alias.Trim().ToLower()
                                                 && sp.MaLoaiSP == loai.MaLoaiSP && sp.Status == 1
                                                 && sp.Hang.Status == 1); // Kiểm tra hãng có status = 1

            if (sanPham == null)
            {
                return HttpNotFound("Không tìm thấy sản phẩm.");
            }

            // Nếu có version, bạn có thể xử lý thêm nếu cần
            if (!string.IsNullOrEmpty(version))
            {
                // Xử lý version nếu cần
            }

            ViewBag.TenSP = sanPham.TenSP;
            ViewBag.Title = sanPham.TenSP;

            return View(sanPham);
        }
    }
}