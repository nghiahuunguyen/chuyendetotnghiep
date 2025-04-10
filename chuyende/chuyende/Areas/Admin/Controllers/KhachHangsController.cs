using chuyende.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace chuyende.Areas.Admin.Controllers
{
    public class KhachHangsController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();
        // GET: Admin/KhachHangs
        public ActionResult Index()
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }
            var danhSachKhachHang = db.HoaDons
                .GroupBy(h => new { h.SoDienThoai, h.Email })
                .Select(g => new KhachHangViewModel
                {
                    TenKH = g.FirstOrDefault().TenKH,
                    Email = g.Key.Email,
                    SoDienThoai = g.Key.SoDienThoai,
                    DiaChi = g.FirstOrDefault().DiaChi,
                    SoLanMua = g.Count() // Tổng số đơn hàng của khách
                })
                .ToList();

            return View(danhSachKhachHang);
        }

    }
}