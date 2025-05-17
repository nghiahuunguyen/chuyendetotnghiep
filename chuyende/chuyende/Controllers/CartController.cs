using chuyende.Helper;
using chuyende.Models;
using chuyende.Other;
using System;
using System.Collections.Generic;
using System.Configuration;
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
            ViewBag.TotalAmount = hoaDon.ChiTietHoaDon.Sum(ct =>
    ((ct.SanPham?.GiaDau ?? 0) - ((ct.SanPham?.GiaDau ?? 0) * (ct.SanPham?.SoGiam ?? 0) / 100)) * ct.SoLuong
);


            // Truyền model sang view Confirmation để xác nhận
            return View("Confirmation", hoaDon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CompletePayment(HoaDon hoaDon)
        {
            if (Session["User"] == null)
                return RedirectToAction("Index", "Login");

            var user = (KhachHang)Session["User"];

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

            // Cập nhật số lượng sản phẩm
            foreach (var ct in chiTiets)
            {
                var sp = db.SanPhams.FirstOrDefault(s => s.MaSP == ct.MaSP);
                if (sp != null)
                {
                    sp.SoLuong -= ct.SoLuong;
                    if (sp.SoLuong < 0) sp.SoLuong = 0;
                }
            }

            db.HoaDons.Add(hoaDon);
            db.SaveChanges();

            // Xóa sản phẩm đã mua khỏi giỏ hàng
            db.ChiTietGioHangs.RemoveRange(chiTiets);
            if (!db.ChiTietGioHangs.Any(c => c.MaGioHang == gioHang.MaGioHang))
                db.GioHangs.Remove(gioHang);
            db.SaveChanges();

            Session["SelectedItems"] = null;

            // Gửi email xác nhận
            decimal tongTien = hoaDon.ChiTietHoaDon.Sum(ct =>
            {
                var sp = db.SanPhams.Find(ct.MaSP);
                decimal gia = (sp?.GiaDau ?? 0) * (1 - (decimal)(sp?.SoGiam ?? 0) / 100);
                return gia * ct.SoLuong;
            });

            string emailBody = $"<div style='color: black;'>" +
                $"<p>Chào {hoaDon.TenKH},</p>" +
                $"<p>Bạn đã đặt hàng thành công vào lúc {hoaDon.NgayTao:HH:mm:ss dd/MM/yyyy}.</p>" +
                $"<p>Mã đơn hàng của bạn là: <strong>{hoaDon.MaHD}</strong></p>" +
                "<p>Chi tiết đơn hàng:</p>" +
                "<table border='1' cellspacing='0' cellpadding='5' style='border-collapse: collapse; width: 100%;'>" +
                "<thead><tr>" +
                "<th style='text-align:left;'>Tên sản phẩm</th>" +
                "<th>Số lượng</th>" +
                "<th>Đơn giá</th>" +
                "<th>Thành tiền</th>" +
                "</tr></thead><tbody>";

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

            emailBody += $"<tr>" +
                         $"<td colspan='3' style='text-align:right; font-weight:bold;'>Tổng cộng:</td>" +
                         $"<td style='text-align:right; font-weight:bold;'>{tongTien:N0}₫</td>" +
                         "</tr></tbody></table>" +
                         "<p><strong>Cảm ơn quý khách đã tin tưởng dịch vụ và mua sắm tại cửa hàng!</strong></p>" +
                         "<p>Gọi mua hàng: <strong>0366 541 719</strong> (7:30 - 22:00)<br/>" +
                         "Bảo hành: <strong>0366 541 718</strong> (8:00 - 21:00)</p>" +
                         "<p>Chúng tôi sẽ sớm liên hệ với bạn để xác nhận đơn hàng và giao hàng trong thời gian sớm nhất.</p>" +
                         "<p><em>Trân trọng,</em><br/>ELECTRONICS STORE</p></div>";

            new SendMail().SendMailFunction(hoaDon.Email, "Xác nhận đơn hàng ELECTRONICS STORE", emailBody);

            // Tạo thông tin thanh toán VNPAY
            string formattedAmount = ((long)(tongTien * 100)).ToString(); // nhân 100 đúng chuẩn VNPAY

            string url = ConfigurationManager.AppSettings["Url"];
            string returnUrl = ConfigurationManager.AppSettings["ReturnUrl"];
            string tmnCode = ConfigurationManager.AppSettings["TmnCode"];
            string hashSecret = ConfigurationManager.AppSettings["HashSecret"];

            PayLib pay = new PayLib();
            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", tmnCode);
            pay.AddRequestData("vnp_Amount", formattedAmount);
            pay.AddRequestData("vnp_BankCode", "");
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_IpAddr", Util.GetIpAddress());
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", $"Thanh toán đơn hàng {hoaDon.MaHD}");
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", returnUrl);
            pay.AddRequestData("vnp_TxnRef", hoaDon.MaHD); // dùng mã đơn hàng làm mã giao dịch

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);

            return Redirect(paymentUrl);
        }

        public ActionResult PaymentConfirm()
        {
            if (Request.QueryString.Count > 0)
            {
                string hashSecret = ConfigurationManager.AppSettings["HashSecret"]; //Chuỗi bí mật
                var vnpayData = Request.QueryString;
                PayLib pay = new PayLib();

                //lấy toàn bộ dữ liệu được trả về
                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        pay.AddResponseData(s, vnpayData[s]);
                    }
                }

                string txnRef = pay.GetResponseData("vnp_TxnRef");
                long orderId;
                if (!long.TryParse(txnRef, out orderId))
                {
                    ViewBag.Message = "Mã đơn hàng (vnp_TxnRef) không hợp lệ: " + txnRef;
                    return View();
                }

                long vnpayTranId = Convert.ToInt64(pay.GetResponseData("vnp_TransactionNo")); //mã giao dịch tại hệ thống VNPAY
                string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode"); //response code: 00 - thành công, khác 00 - xem thêm https://sandbox.vnpayment.vn/apis/docs/bang-ma-loi/
                string vnp_SecureHash = Request.QueryString["vnp_SecureHash"]; //hash của dữ liệu trả về

                bool checkSignature = pay.ValidateSignature(vnp_SecureHash, hashSecret); //check chữ ký đúng hay không?

                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00")
                    {
                        //Thanh toán thành công
                        ViewBag.Message = "Thanh toán thành công hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId;
                    }
                    else
                    {
                        //Thanh toán không thành công. Mã lỗi: vnp_ResponseCode
                        ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId + " | Mã lỗi: " + vnp_ResponseCode;
                    }
                }
                else
                {
                    ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý";
                }
            }

            return View();
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