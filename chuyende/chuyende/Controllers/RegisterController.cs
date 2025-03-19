using chuyende.Models;
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
            string confirmPassword = Request.Form["ConfirmPassword"]; // Lấy mật khẩu xác nhận từ form

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

            // Băm mật khẩu
            khachHang.MatKhau = HashPassword(khachHang.MatKhau);

            db.KhachHangs.Add(khachHang);
            try
            {
                db.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
                return View(khachHang); // Hiển thị lại form với lỗi
            }
            return RedirectToAction("Index", "Login");
        }

        // Hàm tạo mã khách hàng tự động
        private string GenerateCustomerCode()
        {
            var lastCustomer = db.KhachHangs.OrderByDescending(kh => kh.MaKH).FirstOrDefault();
            int nextNumber = 1;
            if (lastCustomer != null)
            {
                string lastCode = lastCustomer.MaKH.Substring(2); // Bỏ 'KH' để lấy số
                if (int.TryParse(lastCode, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }
            return $"KH{nextNumber:D3}"; // Format thành KH001, KH002, ...
        }

        // Hàm băm mật khẩu bằng SHA-256
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