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


            TempData["SuccessMessage"] = "Đăng ký thành công!";
            return RedirectToAction("Index", "Login");
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
