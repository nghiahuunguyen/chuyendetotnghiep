using System;
using System.Linq;
using System.Web.Mvc;
using chuyende.Models;

namespace chuyende.Controllers
{
    public class LoginController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public ActionResult Index(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            // Kiểm tra email/số điện thoại hợp lệ trong DB
            var user = db.KhachHangs.FirstOrDefault(k => (k.Email == email || k.SoDienThoai == email) && k.MatKhau == password);

            if (user != null)
            {
                // Lưu session để duy trì đăng nhập
                Session["User"] = user;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Email/Số điện thoại hoặc mật khẩu không đúng.";
            }

            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear(); // Xóa thông tin đăng nhập
            return RedirectToAction("Index");
        }
    }
}
