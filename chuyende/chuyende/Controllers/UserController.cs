using chuyende.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace chuyende.Controllers
{
    public class UserController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();
        // GET: User
        public ActionResult Index()
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Index", "Login"); // Chuyển hướng đến trang đăng nhập
            }

            var user = Session["User"] as KhachHang; // Ép kiểu an toàn
            return View(user);
        }

        // Cập nhật thông tin người dùng
        [HttpPost]
        public ActionResult UpdateUser(KhachHang updatedUser)
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var userSession = Session["User"] as KhachHang;
            if (userSession != null)
            {
                var userDb = db.KhachHangs.Find(userSession.MaKH);
                if (userDb != null)
                {
                    userDb.TenKH = updatedUser.TenKH;
                    userDb.SoDienThoai = updatedUser.SoDienThoai;
                    userDb.Email = updatedUser.Email;
                    userDb.DiaChi = updatedUser.DiaChi;

                    db.Entry(userDb).State = EntityState.Modified;
                    db.SaveChanges();

                    Session["User"] = userDb;
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin tài khoản!";
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ChangePassword(string CurrentPassword, string NewPassword, string ConfirmNewPassword)
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var userSession = Session["User"] as KhachHang;
            if (userSession == null)
            {
                return RedirectToAction("Index");
            }

            var userDb = db.KhachHangs.Find(userSession.MaKH);
            if (userDb == null)
            {
                return RedirectToAction("Index");
            }

            string hashedCurrentPassword = HashPassword(CurrentPassword);

            if (userDb.MatKhau != hashedCurrentPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu hiện tại không đúng!";
                return RedirectToAction("Index");
            }

            if (NewPassword != ConfirmNewPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu mới và xác nhận mật khẩu không khớp!";
                return RedirectToAction("Index");
            }

            userDb.MatKhau = HashPassword(NewPassword);
            db.Entry(userDb).State = EntityState.Modified;
            db.SaveChanges();

            Session["User"] = userDb;
            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";

            return RedirectToAction("Index");
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
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}