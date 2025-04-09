using System;
using System.Linq;
using System.Web.Mvc;
using chuyende.Helper;
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

            var userExists = db.KhachHangs.Any(k => k.Email == email || k.SoDienThoai == email);
            if (!userExists)
            {
                ViewBag.ErrorMessage = "Tài khoản chưa tồn tại.";
                return View();
            }

            string hashedPassword = HashPassword(password); // Băm mật khẩu trước khi kiểm tra

            var user = db.KhachHangs.FirstOrDefault(k =>
                (k.Email == email || k.SoDienThoai == email) && k.MatKhau == hashedPassword);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Email/Số điện thoại hoặc mật khẩu không đúng.";
                return View();
            }

            if (!user.IsActive) // Kiểm tra tài khoản đã kích hoạt chưa
            {
                ViewBag.ErrorMessage = "Tài khoản không tồn tại.";
                return View();
            }

            // Nếu hợp lệ, lưu vào session và chuyển hướng
            Session["User"] = user;
            return RedirectToAction("Index", "Home");
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
            // Giữ lại giỏ hàng
            var cart = Session["Cart"];

            // Chỉ xóa thông tin đăng nhập
            Session.Remove("User");

            // Khôi phục giỏ hàng
            Session["Cart"] = cart;

            return RedirectToAction("Index", "Home");
        }

        private string GenerateOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString(); // Mã OTP 6 chữ số
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ForgotPassword(string email)
        {
            var user = db.KhachHangs.FirstOrDefault(kh => kh.Email == email);

            if (user == null || !user.IsActive)
            {
                ViewBag.ErrorMessage = "Tài khoản không tồn tại.";
                return View();
            }

            // Tạo mã OTP 6 số
            user.ActivationToken = GenerateOTP();
            db.SaveChanges();

            // Gửi email chứa mã OTP
            string emailBody = $"<p>Chào {user.TenKH},</p><p>Mã xác nhận đặt lại mật khẩu của bạn là: <strong>{user.ActivationToken}</strong>. Vui lòng không chia sẻ mã này bất kỳ ai.</p>";
            SendMail sendMail = new SendMail();
            sendMail.SendMailFunction(user.Email, "Mã xác nhận đặt lại mật khẩu", emailBody);

            return RedirectToAction("ResetPassword", new { email = user.Email });
        }
        public ActionResult ResetPassword(string email)
        {
            ViewBag.Email = email;
            return View();
        }
        [HttpPost]
        public ActionResult ResetPassword(string email, string otp)
        {
            var user = db.KhachHangs.FirstOrDefault(kh => kh.Email == email && kh.ActivationToken == otp);

            if (user == null)
            {
                ViewBag.Email = email;
                ViewBag.ErrorMessage = "Mã xác nhận không hợp lệ!";
                return View();
            }

            return RedirectToAction("NewPassword", new { email = user.Email });
        }
        public ActionResult NewPassword(string email)
        {
            ViewBag.Email = email;
            return View();
        }
        [HttpPost]
        public ActionResult NewPassword(string email, string newPassword, string confirmPassword)
        {
            ViewBag.Email = email; // giữ lại email cho form

            // Kiểm tra rỗng
            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ mật khẩu!";
                return View();
            }

            // Kiểm tra xác nhận không trùng khớp
            if (newPassword != confirmPassword)
            {
                ViewBag.ConfirmPasswordError = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            // Kiểm tra tồn tại người dùng
            var user = db.KhachHangs.FirstOrDefault(kh => kh.Email == email);
            if (user == null)
            {
                ViewBag.ErrorMessage = "Email không hợp lệ!";
                return View();
            }

            // Cập nhật mật khẩu
            user.MatKhau = HashPassword(newPassword);
            user.ActivationToken = null;
            db.SaveChanges();

            TempData["SuccessMessage"] = "Cập nhật mật khẩu thành công!";
            return RedirectToAction("Index", "Login");

        }

    }
}
