using chuyende.Helper;
using chuyende.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace chuyende.Controllers
{
    public class CartController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        // Hàm lấy mã giỏ hàng từ người dùng đăng nhập
        private string GetCartId()
        {
            var user = Session["User"] as KhachHang;
            if (user == null) return null;

            var gioHang = db.GioHangs.FirstOrDefault(g => g.MaKH == user.MaKH);
            if (gioHang != null) return gioHang.MaGioHang;

            // Nếu chưa có giỏ hàng, tạo mới
            gioHang = new GioHang
            {
                MaGioHang = Guid.NewGuid().ToString(),
                MaKH = user.MaKH
            };
            db.GioHangs.Add(gioHang);
            db.SaveChanges();
            return gioHang.MaGioHang;
        }

        public ActionResult Index()
        {
            if (Session["User"] == null)
            {
                TempData["ReturnUrl"] = Url.Action("Index", "Login");
                TempData["Message"] = "Vui lòng đăng nhập để xem giỏ hàng.";
                return RedirectToAction("Index", "Login");
            }

            string maKH = (Session["User"] as KhachHang).MaKH;
            var gioHang = db.GioHangs.FirstOrDefault(g => g.MaKH == maKH);

            if (gioHang == null)
                return View(new List<ChiTietGioHang>());

            var chiTiets = db.ChiTietGioHangs
                .Where(c => c.MaGioHang == gioHang.MaGioHang)
                .ToList();

            foreach (var item in chiTiets)
                item.SanPham = db.SanPhams.Find(item.MaSP);

            // Lấy danh sách sản phẩm được chọn từ Session
            var selectedItems = Session["SelectedItems"] as List<string> ?? new List<string>();

            // Tính tổng tiền chỉ cho các sản phẩm được chọn
            ViewBag.TongTien = chiTiets
                .Where(c => selectedItems.Contains(c.MaSP))
                .Sum(c =>
                {
                    var sp = c.SanPham;
                    decimal gia = (sp.GiaDau ?? 0) * (1 - (decimal)(sp.SoGiam ?? 0) / 100);
                    return gia * c.SoLuong;
                });

            // Đếm số lượng sản phẩm được chọn
            ViewBag.SelectedCount = chiTiets.Count(c => selectedItems.Contains(c.MaSP));

            // Truyền danh sách sản phẩm được chọn để hiển thị checkbox
            ViewBag.SelectedItems = selectedItems;

            return View(chiTiets);
        }

        [HttpPost]
        public ActionResult ToggleSelection(string id, bool isSelected)
        {
            // Lấy danh sách sản phẩm được chọn từ Session
            var selectedItems = Session["SelectedItems"] as List<string> ?? new List<string>();

            if (isSelected && !selectedItems.Contains(id))
            {
                selectedItems.Add(id);
            }
            else if (!isSelected && selectedItems.Contains(id))
            {
                selectedItems.Remove(id);
            }

            // Lưu lại vào Session
            Session["SelectedItems"] = selectedItems;

            return Json(new { success = true });
        }

        public ActionResult AddToCart(string id)
        {
            if (Session["User"] == null)
            {
                TempData["Message"] = "Bạn cần đăng nhập để thêm sản phẩm.";
                return RedirectToAction("Index", "Login");
            }

            var product = db.SanPhams.Find(id);
            if (product == null) return HttpNotFound();

            string cartId = GetCartId();
            var chiTiet = db.ChiTietGioHangs.FirstOrDefault(c => c.MaGioHang == cartId && c.MaSP == id);

            if (chiTiet == null)
            {
                chiTiet = new ChiTietGioHang
                {
                    MaChiTiet = Guid.NewGuid().ToString(),
                    MaGioHang = cartId,
                    MaSP = id,
                    SoLuong = 1
                };
                db.ChiTietGioHangs.Add(chiTiet);
            }
            else
            {
                chiTiet.SoLuong++;
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UpdateQuantity(string id, string action)
        {
            string cartId = GetCartId();
            var item = db.ChiTietGioHangs.FirstOrDefault(x => x.MaSP == id && x.MaGioHang == cartId);

            if (item != null)
            {
                if (action == "increase")
                {
                    item.SoLuong++;
                }
                else if (action == "decrease")
                {
                    item.SoLuong--;
                }

                if (item.SoLuong <= 0)
                {
                    db.ChiTietGioHangs.Remove(item);
                }

                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }



        public ActionResult RemoveFromCart(string id)
        {
            string cartId = GetCartId();
            var chiTiet = db.ChiTietGioHangs.FirstOrDefault(c => c.MaSP == id && c.MaGioHang == cartId);

            if (chiTiet != null)
            {
                db.ChiTietGioHangs.Remove(chiTiet);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult ClearCart()
        {
            string cartId = GetCartId();
            var chiTiets = db.ChiTietGioHangs.Where(c => c.MaGioHang == cartId);
            db.ChiTietGioHangs.RemoveRange(chiTiets);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Checkout()
        {
            if (Session["User"] == null)
            {
                TempData["Message"] = "Vui lòng đăng nhập để thanh toán.";
                return RedirectToAction("Index", "Login");
            }

            var user = (KhachHang)Session["User"];
            var gioHang = db.GioHangs.FirstOrDefault(g => g.MaKH == user.MaKH);

            if (gioHang == null)
            {
                TempData["Message"] = "Giỏ hàng của bạn trống.";
                return RedirectToAction("Index", "Cart");
            }

            // Lấy danh sách sản phẩm được chọn từ Session
            var selectedItems = Session["SelectedItems"] as List<string> ?? new List<string>();

            var chiTiets = db.ChiTietGioHangs
                .Where(c => c.MaGioHang == gioHang.MaGioHang && selectedItems.Contains(c.MaSP))
                .ToList();

            if (!chiTiets.Any())
            {
                TempData["Message"] = "Vui lòng chọn ít nhất một sản phẩm để thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            foreach (var ct in chiTiets)
            {
                ct.SanPham = db.SanPhams.Find(ct.MaSP);
            }

            var hoaDon = new HoaDon
            {
                MaHD = Guid.NewGuid().ToString(),
                TenKH = user.TenKH,
                SoDienThoai = user.SoDienThoai,
                Email = user.Email,
                DiaChi = user.DiaChi,
                PhuongThucThanhToan = 3,
                TrangThai = 1,
                NguoiTao = user.MaKH,
                NgayTao = DateTime.Now,
                ChiTietHoaDon = chiTiets.Select(ct => new ChiTietHoaDon
                {
                    ID = Guid.NewGuid().ToString(),
                    MaSP = ct.MaSP,
                    SoLuong = ct.SoLuong,
                    SanPham = ct.SanPham
                }).ToList()
            };

            // Tính tổng tiền
            ViewBag.TotalAmount = hoaDon.ChiTietHoaDon.Sum(ct => (ct.SanPham?.GiaDau ?? 0) * ct.SoLuong);

            // Truyền model sang view Confirmation để xác nhận
            return View("Confirmation", hoaDon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CompletePayment(HoaDon hoaDon)
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var user = (KhachHang)Session["User"];

            // Kiểm tra địa chỉ
            if (string.IsNullOrWhiteSpace(user.DiaChi))
            {
                TempData["ErrorMessage"] = "Vui lòng cập nhật địa chỉ trước khi thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            hoaDon.MaHD = Guid.NewGuid().ToString();
            hoaDon.NguoiTao = user.MaKH;
            hoaDon.NgayTao = DateTime.Now;
            hoaDon.TrangThai = 1;

            hoaDon.TenKH = user.TenKH;
            hoaDon.SoDienThoai = user.SoDienThoai;
            hoaDon.Email = user.Email;
            hoaDon.DiaChi = user.DiaChi;

            var gioHang = db.GioHangs.FirstOrDefault(g => g.MaKH == user.MaKH);
            var selectedItems = Session["SelectedItems"] as List<string> ?? new List<string>();

            var chiTiets = db.ChiTietGioHangs
                .Include("SanPham")
                .Where(c => c.MaGioHang == gioHang.MaGioHang && selectedItems.Contains(c.MaSP))
                .ToList();

            if (!chiTiets.Any())
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một sản phẩm để thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            hoaDon.ChiTietHoaDon = chiTiets.Select(ct => new ChiTietHoaDon
            {
                ID = Guid.NewGuid().ToString(),
                MaHD = hoaDon.MaHD,
                MaSP = ct.MaSP,
                SoLuong = ct.SoLuong
            }).ToList();

            foreach (var ct in chiTiets)
            {
                var sanPham = db.SanPhams.FirstOrDefault(sp => sp.MaSP == ct.MaSP);
                if (sanPham != null)
                {
                    sanPham.SoLuong -= ct.SoLuong;
                    if (sanPham.SoLuong < 0) sanPham.SoLuong = 0;
                }
            }

            db.HoaDons.Add(hoaDon);
            db.SaveChanges();

            // Xóa các sản phẩm được chọn khỏi giỏ hàng
            db.ChiTietGioHangs.RemoveRange(chiTiets);
            if (!db.ChiTietGioHangs.Any(c => c.MaGioHang == gioHang.MaGioHang))
            {
                db.GioHangs.Remove(gioHang);
            }
            db.SaveChanges();

            // Xóa danh sách sản phẩm được chọn khỏi Session
            Session["SelectedItems"] = null;

            // Gửi email xác nhận
            string emailBody = $"<div style='color: black;'>" +
                $"<p>Chào {hoaDon.TenKH},</p>" +
                $"<p>Bạn đã đặt hàng thành công vào lúc {hoaDon.NgayTao:HH:mm:ss dd/MM/yyyy}.</p>" +
                $"<p>Mã đơn hàng của bạn là: <strong>{hoaDon.MaHD}</strong></p>" +
                "<p>Chi tiết đơn hàng:</p>" +
                "<table border='1' cellspacing='0' cellpadding='5' style='border-collapse: collapse; width: 100%;'>" +
                "<thead>" +
                "<tr>" +
                "<th style='text-align:left;'>Tên sản phẩm</th>" +
                "<th>Số lượng</th>" +
                "<th>Đơn giá</th>" +
                "<th>Thành tiền</th>" +
                "</tr>" +
                "</thead><tbody>";

            foreach (var ct in hoaDon.ChiTietHoaDon)
            {
                var sp = db.SanPhams.Find(ct.MaSP);
                if (sp != null)
                {
                    decimal donGia = (sp.GiaDau ?? 0) * (1 - (decimal)(sp.SoGiam ?? 0) / 100);
                    decimal thanhTien = donGia * ct.SoLuong;

                    emailBody += $"<tr>" +
                                 $"<td>{sp.TenSP}</td>" +
                                 $"<td style='text-align:center;'>{ct.SoLuong}</td>" +
                                 $"<td style='text-align:right;'>{donGia:N0}₫</td>" +
                                 $"<td style='text-align:right;'>{thanhTien:N0}₫</td>" +
                                 "</tr>";
                }
            }

            decimal tongTien = hoaDon.ChiTietHoaDon.Sum(ct =>
            {
                var sp = db.SanPhams.Find(ct.MaSP);
                decimal gia = (sp?.GiaDau ?? 0) * (1 - (decimal)(sp?.SoGiam ?? 0) / 100);
                return gia * ct.SoLuong;
            });

            emailBody += $"<tr>" +
                         $"<td colspan='3' style='text-align:right; font-weight:bold;'>Tổng cộng:</td>" +
                         $"<td style='text-align:right; font-weight:bold;'>{tongTien:N0}₫</td>" +
                         "</tr>";

            emailBody += "</tbody></table>" +
                         "<p><strong>Cảm ơn quý khách đã tin tưởng dịch vụ và mua sắm tại cửa hàng!</strong></p>" +
                         "<p>Gọi mua hàng: <strong>0366 541 719</strong> (7:30 - 22:00)<br/>" +
                         "Bảo hành: <strong>0366 541 718</strong> (8:00 - 21:00)</p>" +
                         "<p>Chúng tôi sẽ sớm liên hệ với bạn để xác nhận đơn hàng và giao hàng trong thời gian sớm nhất.</p>" +
                         "<p><em>Trân trọng,</em><br/>ELECTRONICS STORE</p>" +
                         "</div>";

            SendMail sendMail = new SendMail();
            sendMail.SendMailFunction(hoaDon.Email, "Xác nhận đơn hàng ELECTRONICS STORE", emailBody);

            TempData["Success"] = "Đặt hàng thành công!";
            return RedirectToAction("Index", "Home");
        }


        public ActionResult Confirmation(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Message"] = "Mã hóa đơn không hợp lệ.";
                return RedirectToAction("Index", "Cart");
            }

            var hoaDon = db.HoaDons
                .Include("ChiTietHoaDon.SanPham")
                .FirstOrDefault(h => h.MaHD == id);

            if (hoaDon == null)
            {
                TempData["Message"] = "Hóa đơn không tồn tại.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.TotalAmount = hoaDon.ChiTietHoaDon.Sum(ct => (ct.SanPham?.GiaDau ?? 0) * ct.SoLuong);

            return View(hoaDon);
        }

        public int GetTotalItemsInCart()
        {
            if (Session["User"] == null)
                return 0;

            string maKH = (Session["User"] as KhachHang).MaKH;
            var gioHang = db.GioHangs.FirstOrDefault(g => g.MaKH == maKH);

            if (gioHang == null)
                return 0;

            return db.ChiTietGioHangs
                     .Where(c => c.MaGioHang == gioHang.MaGioHang)
                     .Sum(c => (int?)c.SoLuong) ?? 0;
        }

        public ActionResult GetCartCount()
        {
            ViewBag.CartCount = GetTotalItemsInCart();
            return PartialView("_CartIcon");
        }


    }
}