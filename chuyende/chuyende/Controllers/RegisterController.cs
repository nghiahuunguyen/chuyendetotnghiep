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

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(KhachHang khachHang)
        {
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
            khachHang.MaKH = GenerateCustomerCode();
            khachHang.MatKhau = HashPassword(khachHang.MatKhau);
            khachHang.IsActive = false;
            khachHang.ActivationToken = GenerateOTP();

            db.KhachHangs.Add(khachHang);
            db.SaveChanges();

            // Gửi email xác nhận OTP
            string emailBody = $@"
                                <p>Chào {khachHang.TenKH},</p>
                                <p>Mã xác nhận của bạn là: <strong>{khachHang.ActivationToken}</strong>.</p>
                                <p>Vui lòng không chia sẻ mã này với bất kỳ ai.</p>";

            SendMail sender = new SendMail();
            sender.SendMailFunction(khachHang.Email, "Mã xác nhận đăng ký", emailBody);

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
                TempData["SuccessMessage"] = "Bạn đã đăng ký thành công!.";
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
