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
        [HttpGet]
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

            string hashedPassword = HashPassword(password); // Băm mật khẩu trước khi kiểm tra

            var user = db.KhachHangs.FirstOrDefault(k =>
                (k.Email == email || k.SoDienThoai == email) && k.MatKhau == hashedPassword);

            if (user != null)
            {
                Session["User"] = user;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Email/Số điện thoại hoặc mật khẩu không đúng.";
            }

            return View();
        }

        // Hàm băm mật khẩu SHA-256
        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        public ActionResult Logout()
        {
            Session.Clear(); // Xóa thông tin đăng nhập
            return RedirectToAction("Index");
        }
    }
}
