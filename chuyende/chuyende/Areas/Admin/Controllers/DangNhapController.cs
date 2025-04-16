using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using chuyende.Models;
using System.Diagnostics;
using System;

namespace chuyende.Areas.Admin.Controllers
{
    public class DangNhapController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string TenDN, string MatKhau)
        {
            if (string.IsNullOrEmpty(TenDN) || string.IsNullOrEmpty(MatKhau))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập tên đăng nhập và mật khẩu!";
                return View();
            }

            var user = db.NhanViens.Include(nv => nv.ChucVu)
                                  .FirstOrDefault(nv => nv.TenDN == TenDN && nv.MatKhau == MatKhau);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Tên đăng nhập hoặc mật khẩu không đúng!";
                return View();
            }

            Session["Admin"] = user.TenNV ?? "Unknown";
            Session["TenDN"] = user.TenDN ?? throw new Exception("TenDN không được null");
            Session["MaChucVu"] = user.ChucVu?.MaCV?.Trim() ?? "";
            Session["TenChucVu"] = user.ChucVu?.TenCV?.Trim() ?? "";

            System.Diagnostics.Debug.WriteLine($"Đăng nhập: TenDN={user.TenDN}, TenNV={user.TenNV}, MaNV={user.MaNV}");
            return RedirectToAction("Index", "HomeAdmin");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
