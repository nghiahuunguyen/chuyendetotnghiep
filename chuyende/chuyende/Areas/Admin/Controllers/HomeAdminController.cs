using chuyende.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace chuyende.Areas.Admin.Controllers
{
    public class HomeAdminController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();
        // GET: Admin/HomeAdmin
        public ActionResult Index()
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }
            var model = new ThongKeAdmin
            {
                DonHangChoXuLy = db.HoaDons.Count(hd => hd.TrangThai == 1),
                DonHangDangVanChuyen = db.HoaDons.Count(hd => hd.TrangThai == 2),
                DonHangDaHoanThanh = db.HoaDons.Count(hd => hd.TrangThai == 3 || hd.TrangThai == 0)
            };
            return View(model);
        }
    }
}