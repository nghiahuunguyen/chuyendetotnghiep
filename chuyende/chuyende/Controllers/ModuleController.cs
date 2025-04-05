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

            // Kiểm tra loại sản phẩm
            var loai = db.LoaiSanPhams
                         .FirstOrDefault(l => l.Link.Trim().ToLower() == alias.Trim().ToLower());

            if (loai == null)
            {
                return HttpNotFound("Không tìm thấy loại sản phẩm.");
            }

            // Lấy danh sách sản phẩm theo loại
            var sanPhams = db.SanPhams
                             .Where(sp => sp.MaLoaiSP == loai.MaLoaiSP && sp.Status == 1)
                             .ToList();

            // Gán tên loại sản phẩm vào ViewBag
            ViewBag.TenLoaiSP = loai.TenLoaiSP;
            ViewBag.Title = loai.TenLoaiSP; // Cập nhật ViewBag.Title với tên loại sản phẩm

            return View(sanPhams);
        }
        public ActionResult ChiTiet(string loaiAlias, string alias, string version)
        {
            if (string.IsNullOrEmpty(alias))
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

            // Tìm sản phẩm theo alias trong loại sản phẩm
            var sanPham = db.SanPhams
                            .FirstOrDefault(sp => sp.Link.Trim().ToLower() == alias.Trim().ToLower()
                                                 && sp.MaLoaiSP == loai.MaLoaiSP && sp.Status == 1);

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
