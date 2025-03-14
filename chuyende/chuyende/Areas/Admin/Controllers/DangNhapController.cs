using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using chuyende.Models;

namespace chuyende.Areas.Admin.Controllers
{
    public class DangNhapController : Controller
    {
        private QuanLyBanDienTuEntities1 db = new QuanLyBanDienTuEntities1();

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

            var user = db.NhanVien.Include(nv => nv.ChucVu)
                                  .FirstOrDefault(nv => nv.TenDN == TenDN && nv.MatKhau == MatKhau);

            if (user == null)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
                return View();
            }

            Session["User"] = user.TenNV;
            Session["ChucVu"] = user.ChucVu?.TenCV?.Trim() ?? "Không xác định";

            return RedirectToAction("Index", "HomeAdmin");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
