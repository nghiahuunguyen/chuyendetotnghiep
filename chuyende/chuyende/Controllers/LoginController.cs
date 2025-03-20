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
                ViewBag.ErrorMessage = "Tài khoản chưa được kích hoạt. Vui lòng kiểm tra email để xác nhận.";
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
            Session.Clear(); // Xóa thông tin đăng nhập
            return RedirectToAction("Index");
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
                ViewBag.ErrorMessage = "Email không tồn tại hoặc tài khoản chưa kích hoạt.";
                return View();
            }

            // Tạo mã OTP 6 số
            user.ActivationToken = GenerateOTP();
            db.SaveChanges();

            // Gửi email chứa mã OTP
            string emailBody = $"<p>Chào {user.TenKH},</p><p>Mã xác nhận đặt lại mật khẩu của bạn là: <strong>{user.ActivationToken}</strong></p>";
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
            if (newPassword != confirmPassword)
            {
                ViewBag.ErrorMessage = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            var user = db.KhachHangs.FirstOrDefault(kh => kh.Email == email);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Email không hợp lệ!";
                return View();
            }

            // Cập nhật mật khẩu mới (đã băm)
            user.MatKhau = HashPassword(newPassword);
            user.ActivationToken = null; // Xóa OTP sau khi đổi mật khẩu
            db.SaveChanges();

            return RedirectToAction("Index", "Login");
        }
    

    }
}
