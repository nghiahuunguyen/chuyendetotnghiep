using chuyende.Helper;
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

        public ActionResult ViewOrder()
        {
            var user = Session["User"] as KhachHang;
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Lọc hóa đơn theo Email khách hàng
            var donHangs = db.HoaDons
                             .Where(h => h.Email == user.Email)
                             .OrderByDescending(h => h.NgayTao)
                             .ToList();

            return View(donHangs);
        }

        public ActionResult OrderDetails(string id)
        {
            var hoaDon = db.HoaDons
                           .Include("ChiTietHoaDon.SanPham")
                           .FirstOrDefault(h => h.MaHD == id);

            if (hoaDon == null)
            {
                return HttpNotFound();
            }

            return View(hoaDon);
        }

        public ActionResult CancelOrder(string id)
        {
            // Kiểm tra id hợp lệ
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Mã đơn hàng không hợp lệ.";
                return RedirectToAction("ViewOrder");
            }

            // Tìm đơn hàng
            var order = db.HoaDons.Find(id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("ViewOrder");
            }

            // Kiểm tra quyền sở hữu đơn hàng
            var user = Session["User"] as KhachHang;
            if (user == null || order.Email != user.Email)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền hủy đơn hàng này.";
                return RedirectToAction("ViewOrder");
            }

            // Kiểm tra trạng thái đơn hàng
            if (order.TrangThai != 1)
            {
                TempData["ErrorMessage"] = "Đơn hàng không thể hủy vì không ở trạng thái Đang xử lý.";
                return RedirectToAction("ViewOrder");
            }

            // Cập nhật trạng thái thành "Đã hủy"
            order.TrangThai = 4;
            db.SaveChanges();

            string emailBody = $"<div style='color: black; font-family: Arial, sans-serif;'>"
    + $"<p>Chào {order.TenKH},</p>"
    + $"<p>Đơn hàng của bạn với mã <strong>{order.MaHD}</strong> đã được hủy thành công vào lúc {DateTime.Now:HH:mm:ss dd/MM/yyyy}.</p>"
    + "<p>Chi tiết đơn hàng:</p>"
    + "<table border='1' cellspacing='0' cellpadding='5' style='border-collapse: collapse; width: 100%; color: black;'>"
    + "<thead>"
    + "<tr>"
    + "<th style='text-align:left; color: black;'>Tên sản phẩm</th>"
    + "<th style='color: black;'>Số lượng</th>"
    + "<th style='color: black;'>Đơn giá</th>"
    + "<th style='color: black;'>Thành tiền</th>"
    + "</tr>"
    + "</thead><tbody>";

            foreach (var item in order.ChiTietHoaDon)
            {
                decimal giaDau = item.SanPham.GiaDau ?? 0;
                decimal soGiam = item.SanPham.SoGiam ?? 0;
                decimal donGia = giaDau - soGiam;
                decimal thanhTien = item.SoLuong * donGia;

                emailBody += "<tr>"
                    + $"<td style='color: black;'>{item.SanPham.TenSP}</td>"
                    + $"<td style='text-align:center; color: black;'>{item.SoLuong}</td>"
                    + $"<td style='text-align:right; color: black;'>{String.Format("{0:C0}", donGia)}</td>"
                    + $"<td style='text-align:right; color: black;'>{String.Format("{0:C0}", thanhTien)}</td>"
                    + "</tr>";
            }

            decimal tongTien = order.ChiTietHoaDon.Sum(c => c.SoLuong * ((c.SanPham.GiaDau ?? 0) - (c.SanPham.SoGiam ?? 0)));

            emailBody += "</tbody></table>"
                + $"<p style='color: black;'>Tổng tiền: <strong>{String.Format("{0:C0}", tongTien)}</strong></p>"
                + "<p style='color: black;'><strong>Cảm ơn quý khách đã tin tưởng dịch vụ và mua sắm tại cửa hàng!</strong></p>"
                + "<p style='color: black;'>Gọi mua hàng: <strong>0366 541 719</strong> (7:30 - 22:00)<br/>"
                + "Bảo hành: <strong>0366 541 718</strong> (8:00 - 21:00)</p>"
                + "<p style='color: black;'>Chúng tôi sẽ sớm liên hệ với bạn để xác nhận đơn hàng và giao hàng trong thời gian sớm nhất.</p>"
                + "<p style='color: black;'><em>Trân trọng,</em><br/>ELECTRONICS STORE</p>"
                + "</div>";


            // Gửi email thông báo hủy
            try
            {
                SendMail sendMail = new SendMail();
                sendMail.SendMailFunction(order.Email, $"Thông báo hủy đơn hàng {order.MaHD}", emailBody);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Hủy đơn hàng thành công, nhưng không thể gửi email thông báo.";
                // Log lỗi: Console.WriteLine(ex.Message);
            }

            TempData["SuccessMessage"] = "Đơn hàng đã được hủy thành công.";
            return RedirectToAction("ViewOrder");
        }
    }
}