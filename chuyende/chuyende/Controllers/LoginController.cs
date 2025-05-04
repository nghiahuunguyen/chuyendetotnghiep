using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using chuyende.Helper;
using chuyende.Models;
using Newtonsoft.Json;

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

            string hashedPassword = HashPassword(password);

            var user = db.KhachHangs.FirstOrDefault(k =>
                (k.Email == email || k.SoDienThoai == email) && k.MatKhau == hashedPassword);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Email/Số điện thoại hoặc mật khẩu không đúng.";
                return View();
            }

            if (!user.IsActive)
            {
                ViewBag.ErrorMessage = "Tài khoản chưa được kích hoạt.";
                return View();
            }

            Session["User"] = user;
            return RedirectToAction("Index", "Home");
        }

        // Hàm băm mật khẩu SHA-256
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        // Logout
        public ActionResult Logout()
        {
            var cart = Session["Cart"];
            Session.Remove("User");
            Session["Cart"] = cart;
            return RedirectToAction("Index", "Home");
        }

        // Tạo mã OTP 6 chữ số
        private string GenerateOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        // GET: Forgot Password
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Forgot Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập email.";
                return View();
            }

            var user = db.KhachHangs.FirstOrDefault(kh => kh.Email == email);
            if (user == null || !user.IsActive)
            {
                ViewBag.ErrorMessage = "Tài khoản không tồn tại hoặc chưa được kích hoạt.";
                return View();
            }

            // Tạo OTP và lưu kèm thời gian tạo dưới dạng JSON
            string otp = GenerateOTP();
            var otpData = new
            {
                otp = otp,
                generatedAt = DateTime.UtcNow.ToString("o") // Định dạng ISO 8601
            };
            user.ActivationToken = JsonConvert.SerializeObject(otpData);
            db.SaveChanges();

            // Gửi email chứa mã OTP
            string emailBody = $"<p>Chào {user.TenKH},</p><p>Mã xác nhận đặt lại mật khẩu của bạn là: <strong>{otp}</strong>. Mã này có hiệu lực trong 2 phút. Vui lòng không chia sẻ mã này.</p>";
            SendMail sendMail = new SendMail();
            sendMail.SendMailFunction(user.Email, "Mã xác nhận đặt lại mật khẩu", emailBody);

            return RedirectToAction("ResetPassword", new { email = user.Email });
        }

        // GET: Reset Password
        public ActionResult ResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }
            ViewBag.Email = email;
            return View();
        }

        // POST: Reset Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(string email, string otp)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
            {
                ViewBag.Email = email;
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            var user = db.KhachHangs.FirstOrDefault(kh => kh.Email == email);
            if (user == null || string.IsNullOrEmpty(user.ActivationToken))
            {
                ViewBag.Email = email;
                ViewBag.ErrorMessage = "Email không hợp lệ hoặc mã OTP không tồn tại.";
                return View();
            }

            try
            {
                // Phân tích JSON từ ActivationToken
                var otpData = JsonConvert.DeserializeObject<dynamic>(user.ActivationToken);
                string storedOtp = otpData.otp;
                DateTime generatedAt = DateTime.Parse(otpData.generatedAt.ToString());

                // Kiểm tra OTP
                if (storedOtp != otp)
                {
                    ViewBag.Email = email;
                    ViewBag.ErrorMessage = "Mã OTP không đúng.";
                    return View();
                }

                // Kiểm tra OTP hết hạn (2 phút = 120 giây)
                if ((DateTime.UtcNow - generatedAt).TotalSeconds > 120)
                {
                    ViewBag.Email = email;
                    ViewBag.ErrorMessage = "Mã OTP đã hết hạn. Vui lòng yêu cầu mã mới.";
                    return View();
                }

                return RedirectToAction("NewPassword", new { email = user.Email });
            }
            catch
            {
                ViewBag.Email = email;
                ViewBag.ErrorMessage = "Lỗi xử lý mã OTP. Vui lòng thử lại.";
                return View();
            }
        }

        // POST: Resend OTP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResendOTP(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.ErrorMessage = "Email không hợp lệ.";
                return View("ResetPassword");
            }

            var user = db.KhachHangs.FirstOrDefault(kh => kh.Email == email && kh.IsActive);
            if (user == null || string.IsNullOrEmpty(user.ActivationToken))
            {
                ViewBag.Email = email;
                ViewBag.ErrorMessage = "Tài khoản không tồn tại hoặc không có mã OTP.";
                return View("ResetPassword");
            }

            try
            {
                // Phân tích JSON từ ActivationToken
                var otpData = JsonConvert.DeserializeObject<dynamic>(user.ActivationToken);
                DateTime generatedAt = DateTime.Parse(otpData.generatedAt.ToString());

                // Kiểm tra thời gian chờ 30 giây
                if ((DateTime.UtcNow - generatedAt).TotalSeconds < 30)
                {
                    int remainingSeconds = (int)(30 - (DateTime.UtcNow - generatedAt).TotalSeconds);
                    ViewBag.Email = email;
                    ViewBag.ErrorMessage = $"Vui lòng đợi {remainingSeconds} giây trước khi yêu cầu mã mới.";
                    return View("ResetPassword");
                }

                // Tạo OTP mới
                string newOtp = GenerateOTP();
                var newOtpData = new
                {
                    otp = newOtp,
                    generatedAt = DateTime.UtcNow.ToString("o")
                };
                user.ActivationToken = JsonConvert.SerializeObject(newOtpData);
                db.SaveChanges();

                // Gửi email chứa OTP mới
                string emailBody = $"<p>Chào {user.TenKH},</p><p>Mã xác nhận đặt lại mật khẩu mới của bạn là: <strong>{newOtp}</strong>. Mã này có hiệu lực trong 2 phút. Vui lòng không chia sẻ mã này.</p>";
                SendMail sendMail = new SendMail();
                sendMail.SendMailFunction(user.Email, "Mã xác nhận đặt lại mật khẩu mới", emailBody);

                ViewBag.Email = email;
                ViewBag.ErrorMessage = "Mã OTP mới đã được gửi đến email của bạn!";
                return View("ResetPassword");
            }
            catch
            {
                ViewBag.Email = email;
                ViewBag.ErrorMessage = "Lỗi gửi mã OTP. Vui lòng thử lại.";
                return View("ResetPassword");
            }
        }

        // GET: New Password
        public ActionResult NewPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }
            ViewBag.Email = email;
            return View();
        }

        // POST: New Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewPassword(string email, string newPassword, string confirmPassword)
        {
            ViewBag.Email = email;

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ mật khẩu.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.ErrorMessage = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            var user = db.KhachHangs.FirstOrDefault(kh => kh.Email == email);
            if (user == null)
            {
                ViewBag.ErrorMessage = "Email không hợp lệ.";
                return View();
            }

            user.MatKhau = HashPassword(newPassword);
            user.ActivationToken = null;
            db.SaveChanges();

            TempData["SuccessMessage"] = "Cập nhật mật khẩu thành công!";
            return RedirectToAction("Index");
        }
    }
}