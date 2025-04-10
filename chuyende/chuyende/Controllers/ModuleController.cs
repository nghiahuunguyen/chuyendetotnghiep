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
        public ActionResult ByLoai(string alias)
        {
            if (string.IsNullOrEmpty(alias))
                return HttpNotFound("Alias không hợp lệ.");

            var loai = db.LoaiSanPhams.FirstOrDefault(l => l.Link.Trim().ToLower() == alias.Trim().ToLower() && l.Status == 1);
            if (loai == null)
                return HttpNotFound("Không tìm thấy loại sản phẩm.");

            var sanPhams = db.SanPhams
                             .Where(sp => sp.MaLoaiSP == loai.MaLoaiSP && sp.Status == 1 && sp.Hang.Status == 1)
                             .ToList();

            ViewBag.TenLoaiSP = loai.TenLoaiSP;
            ViewBag.Title = loai.TenLoaiSP;
            ViewBag.LoaiAlias = loai.Link;
            ViewBag.HangSanPhams = sanPhams.Select(sp => sp.Hang).Distinct().ToList();

            return View("ByLoai", sanPhams);
        }

        public ActionResult ByHang(string loaiAlias, string hangAlias)
        {
            if (string.IsNullOrEmpty(loaiAlias) || string.IsNullOrEmpty(hangAlias))
                return HttpNotFound("Alias không hợp lệ.");

            var loai = db.LoaiSanPhams.FirstOrDefault(l => l.Link.Trim().ToLower() == loaiAlias.Trim().ToLower() && l.Status == 1);
            if (loai == null)
                return HttpNotFound("Không tìm thấy loại sản phẩm.");

            var hang = db.Hangs.FirstOrDefault(h => h.Link.Trim().ToLower() == hangAlias.Trim().ToLower() && h.Status == 1);
            if (hang == null)
                return HttpNotFound("Không tìm thấy hãng sản phẩm.");

            var sanPhams = db.SanPhams
                             .Where(sp => sp.MaLoaiSP == loai.MaLoaiSP && sp.MaHang == hang.MaHang && sp.Status == 1)
                             .ToList();

            ViewBag.TenLoaiSP = loai.TenLoaiSP;
            ViewBag.Title = hang.TenHang; 
            ViewBag.LoaiAlias = loai.Link;
            ViewBag.HangAlias = hang.Link;
            ViewBag.HangSanPhams = db.Hangs
                                     .Where(h => h.Status == 1 && h.SanPhams.Any(sp => sp.MaLoaiSP == loai.MaLoaiSP))
                                     .ToList();

            return View("ByHang", sanPhams);
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