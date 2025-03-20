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
            khachHang.ActivationToken = GenerateToken(); // Tạo token xác nhận

            db.KhachHangs.Add(khachHang);
            db.SaveChanges();

            // Gửi email xác nhận
            string activationLink = Url.Action("Activate", "Register", new { token = khachHang.ActivationToken, email = khachHang.Email }, Request.Url.Scheme);
            string emailBody = $"<p>Chào {khachHang.TenKH},</p><p>Nhấn vào <a href='{activationLink}'>đây</a> để kích hoạt tài khoản của bạn.</p>";

            SendMail sendMail = new SendMail();
            sendMail.SendMailFunction(khachHang.Email, "Xác nhận đăng ký", emailBody);

            return RedirectToAction("Index", "Login");
        }

        // Xác nhận tài khoản khi người dùng nhấn vào link xác nhận
        public ActionResult Activate(string token, string email)
        {
            var user = db.KhachHangs.FirstOrDefault(kh => kh.Email == email && kh.ActivationToken == token);

            if (user != null && !user.IsActive)
            {
                user.IsActive = true;
                user.ActivationToken = null; // Xóa token sau khi xác nhận
                db.SaveChanges();
                ViewBag.Message = "Tài khoản của bạn đã được kích hoạt!";
            }
            else
            {
                ViewBag.Message = "Liên kết không hợp lệ hoặc tài khoản đã được kích hoạt.";
            }

            return View();
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

        private string GenerateToken()
        {
            return Guid.NewGuid().ToString();
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
