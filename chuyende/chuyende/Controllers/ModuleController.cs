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
            // Kiểm tra alias nhận được
            if (string.IsNullOrEmpty(alias))
            {
                return HttpNotFound("Alias không hợp lệ.");
            }

            // Kiểm tra loại sản phẩm có status = 1
            var loai = db.LoaiSanPhams
                         .FirstOrDefault(l => l.Link.Trim().ToLower() == alias.Trim().ToLower() && l.Status == 1);

            if (loai == null)
            {
                return HttpNotFound("Không tìm thấy loại sản phẩm.");
            }

            // Lấy danh sách sản phẩm theo loại, chỉ lấy sản phẩm có status = 1 và hãng có status = 1
            var sanPhams = db.SanPhams
                             .Where(sp => sp.MaLoaiSP == loai.MaLoaiSP && sp.Status == 1)
                             .Where(sp => sp.Hang.Status == 1) // Kiểm tra hãng có status = 1
                             .ToList();

            // Gán tên loại sản phẩm vào ViewBag
            ViewBag.TenLoaiSP = loai.TenLoaiSP;
            ViewBag.Title = loai.TenLoaiSP; // Cập nhật ViewBag.Title với tên loại sản phẩm

            // Truyền tất cả các hãng vào ViewBag để hiển thị logo của từng hãng, chỉ lấy hãng có status = 1
            ViewBag.HangSanPhams = sanPhams.Select(sp => sp.Hang).Distinct().ToList();

            return View(sanPhams);
        }

        public ActionResult ChiTiet(string loaiAlias, string alias, string version)
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
        public ActionResult ByHang(string loaiAlias, string hangAlias)
        {
            // Kiểm tra alias nhận được
            if (string.IsNullOrEmpty(loaiAlias) || string.IsNullOrEmpty(hangAlias))
            {
                return HttpNotFound("Alias không hợp lệ.");
            }

            // Kiểm tra loại sản phẩm
            var loai = db.LoaiSanPhams
                         .FirstOrDefault(l => l.Link.Trim().ToLower() == loaiAlias.Trim().ToLower());

            if (loai == null)
            {
                return HttpNotFound("Không tìm thấy loại sản phẩm.");
            }

            // Kiểm tra hãng sản phẩm
            var hang = db.Hangs
                         .FirstOrDefault(h => h.MaHang.Trim().ToLower() == hangAlias.Trim().ToLower());

            if (hang == null)
            {
                return HttpNotFound("Không tìm thấy hãng sản phẩm.");
            }

            // Lấy danh sách sản phẩm theo loại và hãng
            var sanPhams = db.SanPhams
                             .Where(sp => sp.MaLoaiSP == loai.MaLoaiSP && sp.MaHang == hang.MaHang && sp.Status == 1)
                             .ToList();

            // Gán tên loại sản phẩm vào ViewBag
            ViewBag.TenLoaiSP = loai.TenLoaiSP;
            ViewBag.Title = loai.TenLoaiSP;

            // Truyền hãng và sản phẩm vào ViewBag
            ViewBag.HangSanPhams = db.Hangs.Where(h => h.Status == 1).ToList(); // Hiển thị logo các hãng

            return View(sanPhams);
        }
    }
}
