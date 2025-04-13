using chuyende.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;


namespace chuyende.Areas.Admin.Controllers
{
    public class KhachHangsController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();
        // GET: Admin/KhachHangs
        public ActionResult Index(int? page)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }

            int pageSize = 5;
            int pageNumber = (page ?? 1);

            var danhSachKhachHang = db.HoaDons
                .GroupBy(h => new { h.SoDienThoai, h.Email })
                .Select(g => new KhachHangViewModel
                {
                    TenKH = g.FirstOrDefault().TenKH,
                    Email = g.Key.Email,
                    SoDienThoai = g.Key.SoDienThoai,
                    DiaChi = g.FirstOrDefault().DiaChi,
                    SoLanMua = g.Count()
                })
                .OrderByDescending(kh => kh.SoLanMua) // sắp xếp theo số lần mua nếu muốn
                .ToPagedList(pageNumber, pageSize);

            return View(danhSachKhachHang);
        }

    }
}