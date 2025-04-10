using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using chuyende.Models;
using System.Diagnostics;

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
                ViewBag.Error = "Vui lòng nhập tên đăng nhập và mật khẩu!";
                return View();
            }

            var user = db.NhanViens.Include(nv => nv.ChucVu)
                                  .FirstOrDefault(nv => nv.TenDN == TenDN && nv.MatKhau == MatKhau);

            if (user == null)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
                return View();
            }

            Session["Admin"] = user.TenNV;
            Session["MaChucVu"] = user.ChucVu?.MaCV?.Trim();      // Dùng để phân quyền
            Session["TenChucVu"] = user.ChucVu?.TenCV?.Trim();    // Dùng để hiển thị



            return RedirectToAction("Index", "HomeAdmin");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
