using chuyende.Models;
using chuyende.Helper;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json; // Sử dụng Newtonsoft.Json thay cho System.Text.Json

namespace chuyende.Controllers
{
    public class RegisterController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        [HttpGet]
        public ActionResult Index()
        {
            // Hiển thị form đăng ký
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(KhachHang khachHang)
        {
            // Lấy mật khẩu xác nhận từ form
            string confirmPassword = Request.Form["ConfirmPassword"];

            // Kiểm tra Email đã tồn tại
            if (db.KhachHangs.Any(k => k.Email == khachHang.Email && k.IsActive))
            {
                ViewBag.ToastError = "Email đã được sử dụng!";
                return View(khachHang);
            }

            // Kiểm tra SĐT đã tồn tại
            if (db.KhachHangs.Any(k => k.SoDienThoai == khachHang.SoDienThoai && k.IsActive))
            {
                ViewBag.ToastError = "Số điện thoại đã được sử dụng!";
                return View(khachHang);
            }

            // Kiểm tra mật khẩu xác nhận
            if (khachHang.MatKhau != confirmPassword)
            {
                ViewBag.ToastError = "Mật khẩu xác nhận không khớp!";
                return View(khachHang);
            }

            // Kiểm tra tính hợp lệ của Model
            if (!ModelState.IsValid)
            {
                ViewBag.ToastError = "Vui lòng nhập đầy đủ thông tin!";
                return View(khachHang);
            }

            // Tạo khách hàng mới
            khachHang.MaKH = GenerateCustomerCode(); // Tạo mã khách hàng
            khachHang.MatKhau = HashPassword(khachHang.MatKhau); // Mã hóa mật khẩu
            khachHang.IsActive = false; // Chưa kích hoạt tài khoản

            // Tạo OTP và lưu kèm thời gian tạo dưới dạng JSON
            string otp = GenerateOTP();
            var otpData = new
            {
                otp = otp,
                generatedAt = DateTime.UtcNow.ToString("o") // Định dạng ISO 8601
            };
            khachHang.ActivationToken = JsonConvert.SerializeObject(otpData); // Sử dụng Newtonsoft.Json

            db.KhachHangs.Add(khachHang);
            db.SaveChanges(); // Lưu vào cơ sở dữ liệu

            // Gửi email chứa OTP
            string emailBody = $@"
                                <p>Chào {khachHang.TenKH},</p>
                                <p>Mã xác nhận của bạn là: <strong>{otp}</strong>.</p>
                                <p>Mã này có hiệu lực trong 2 phút. Vui lòng không chia sẻ mã này với bất kỳ ai.</p>";

            SendMail sender = new SendMail();
            sender.SendMailFunction(khachHang.Email, "Mã xác nhận đăng ký", emailBody);

            // Chuyển hướng đến trang nhập OTP
            return RedirectToAction("ConfirmOTP", new { email = khachHang.Email });
        }

        // Hiển thị trang nhập mã OTP
        public ActionResult ConfirmOTP(string email)
        {
            ViewBag.Email = email; // Truyền email để hiển thị trên view
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmOTP(string email, string otp)
        {
            // Tìm khách hàng theo email và trạng thái chưa kích hoạt
            var user = db.KhachHangs.FirstOrDefault(kh => kh.Email == email && !kh.IsActive);

            if (user == null || string.IsNullOrEmpty(user.ActivationToken))
            {
                ViewBag.Message = "Email không tồn tại hoặc tài khoản đã được kích hoạt!";
                ViewBag.Email = email;
                return View();
            }

            try
            {
                // Phân tích JSON từ ActivationToken
                var otpData = JsonConvert.DeserializeObject<dynamic>(user.ActivationToken); // Sử dụng dynamic cho đơn giản
                string storedOtp = otpData.otp;
                DateTime generatedAt = DateTime.Parse(otpData.generatedAt.ToString());

                // Kiểm tra OTP có khớp không
                if (storedOtp != otp)
                {
                    ViewBag.Message = "Mã xác nhận không hợp lệ!";
                    ViewBag.Email = email;
                    return View();
                }

                // Kiểm tra OTP có hết hạn không (2 phút = 120 giây)
                if ((DateTime.UtcNow - generatedAt).TotalSeconds > 120)
                {
                    ViewBag.Message = "Mã OTP đã hết hạn! Vui lòng yêu cầu mã mới.";
                    ViewBag.Email = email;
                    return View();
                }

                // OTP hợp lệ, kích hoạt tài khoản
                user.IsActive = true;
                user.ActivationToken = null; // Xóa ActivationToken
                db.SaveChanges();
                TempData["SuccessMessage"] = "Bạn đã đăng ký thành công!";
                return RedirectToAction("Index", "Login");
            }
            catch
            {
                ViewBag.Message = "Lỗi xử lý mã OTP. Vui lòng thử lại.";
                ViewBag.Email = email;
                return View();
            }
        }

        // Xử lý yêu cầu gửi lại OTP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResendOTP(string email)
        {
            // Tìm khách hàng theo email và trạng thái chưa kích hoạt
            var user = db.KhachHangs.FirstOrDefault(kh => kh.Email == email && !kh.IsActive);

            if (user == null || string.IsNullOrEmpty(user.ActivationToken))
            {
                ViewBag.Message = "Email không tồn tại hoặc tài khoản đã được kích hoạt!";
                ViewBag.Email = email;
                return View("ConfirmOTP");
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
                    ViewBag.Message = $"Vui lòng đợi {remainingSeconds} giây trước khi yêu cầu mã mới!";
                    ViewBag.Email = email;
                    return View("ConfirmOTP");
                }

                // Tạo OTP mới
                string newOtp = GenerateOTP();
                var newOtpData = new
                {
                    otp = newOtp,
                    generatedAt = DateTime.UtcNow.ToString("o")
                };
                user.ActivationToken = JsonConvert.SerializeObject(newOtpData); // Sử dụng Newtonsoft.Json
                db.SaveChanges();

                // Gửi email chứa OTP mới
                string emailBody = $@"
                                    <p>Chào {user.TenKH},</p>
                                    <p>Mã xác nhận mới của bạn là: <strong>{newOtp}</strong>.</p>
                                    <p>Mã này có hiệu lực trong 2 phút. Vui lòng không chia sẻ mã này với bất kỳ ai.</p>";

                SendMail sender = new SendMail();
                sender.SendMailFunction(user.Email, "Mã xác nhận đăng ký mới", emailBody);

                ViewBag.Email = email;
                return View("ConfirmOTP");
            }
            catch
            {
                ViewBag.Message = "Lỗi xử lý yêu cầu. Vui lòng thử lại.";
                ViewBag.Email = email;
                return View("ConfirmOTP");
            }
        }

        // Tạo mã khách hàng (KH001, KH002, ...)
        private string GenerateCustomerCode()
        {
            var lastCustomer = db.KhachHangs.OrderByDescending(kh => kh.MaKH).FirstOrDefault();
            int nextNumber = 1;
            if (lastCustomer != null)
            {
                string lastCode = lastCustomer.MaKH.Substring(2);
                if (int.TryParse(lastCode, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }
            return $"KH{nextNumber:D3}";
        }

        // Tạo OTP 6 chữ số
        private string GenerateOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        // Mã hóa mật khẩu bằng SHA256
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}