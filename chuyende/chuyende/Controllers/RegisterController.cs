using chuyende.Models;
using chuyende.Helper;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace chuyende.Controllers
{
    public class RegisterController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(KhachHang khachHang)
        {
            // Kiểm tra Email đã tồn tại hay chưa
            bool emailExists = db.KhachHangs.Any(k => k.Email == khachHang.Email);
            if (emailExists)
            {
                ViewBag.EmailError = "Email đã được sử dụng!";
                return View(khachHang); // Trả về view và hiển thị thông báo lỗi
            }

            // Kiểm tra Số điện thoại đã tồn tại hay chưa
            bool phoneExists = db.KhachHangs.Any(k => k.SoDienThoai == khachHang.SoDienThoai);
            if (phoneExists)
            {
                ViewBag.PhoneError = "Số điện thoại đã được sử dụng!";
                return View(khachHang); // Trả về view và hiển thị thông báo lỗi
            }

            string confirmPassword = Request.Form["ConfirmPassword"];

            if (khachHang.MatKhau != confirmPassword)
            {
                ViewBag.ConfirmPasswordError = "Mật khẩu xác nhận không khớp!";
                return View(khachHang);
            }

            if (!ModelState.IsValid)
            {
                return View(khachHang);
            }

            // Tạo mã khách hàng tự động
            khachHang.MaKH = GenerateCustomerCode();
            khachHang.MatKhau = HashPassword(khachHang.MatKhau);
            khachHang.IsActive = false; // Chưa kích hoạt
            khachHang.ActivationToken = GenerateOTP(); // 🔹 Sử dụng mã OTP thay vì token

            db.KhachHangs.Add(khachHang);
            db.SaveChanges();

            // Gửi email chứa mã OTP
            string emailBody = $"<p>Chào {khachHang.TenKH},</p><p>Mã xác nhận của bạn là: <strong>{khachHang.ActivationToken}</strong>. Vui lòng không chia sẻ mã này bất kỳ ai</p>";
            SendMail sendMail = new SendMail();
            sendMail.SendMailFunction(khachHang.Email, "Mã xác nhận đăng ký", emailBody);

            return RedirectToAction("ConfirmOTP", new { email = khachHang.Email });
        }

        // Trang nhập mã xác nhận OTP
        public ActionResult ConfirmOTP(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public ActionResult ConfirmOTP(string email, string otp)
        {
            var user = db.KhachHangs.FirstOrDefault(kh => kh.Email == email && kh.ActivationToken == otp);

            if (user != null && !user.IsActive)
            {
                user.IsActive = true;
                user.ActivationToken = null; // Xóa OTP sau khi xác nhận
                db.SaveChanges();
                return RedirectToAction("Index", "Login");
            }
            else
            {
                ViewBag.Message = "Mã xác nhận không hợp lệ!";
                return View();
            }
        }

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

        private string GenerateOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString(); // 🔹 OTP 6 chữ số
        }

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
