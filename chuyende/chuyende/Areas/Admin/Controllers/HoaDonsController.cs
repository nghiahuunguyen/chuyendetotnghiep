using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.UI;
using chuyende.Helper;
using chuyende.Models;
using PagedList;

namespace chuyende.Areas.Admin.Controllers
{
    public class HoaDonsController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        public ActionResult Search(string keyword, int? page = 1)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return RedirectToAction("Index");
            }

            var hoaddons = db.HoaDons
                .Include(hd => hd.ChiTietHoaDon)
                .Where(h => h.MaHD.Contains(keyword) || h.TenKH.Contains(keyword)
                            || h.SoDienThoai.Contains(keyword) || h.Email.Contains(keyword))
                .ToList();

            if (hoaddons == null || !hoaddons.Any())
            {
                TempData["ErrorMessage"] = "Không tìm thấy hóa đơn nào phù hợp.";
                return RedirectToAction("Index");
            }

            // Tính tổng tiền cho mỗi hóa đơn, áp dụng giảm giá
            List<decimal> tongTienList = new List<decimal>();
            foreach (var hoaDon in hoaddons)
            {
                decimal tongTien = 0;
                foreach (var chiTiet in hoaDon.ChiTietHoaDon)
                {
                    var sanPham = db.SanPhams.Find(chiTiet.MaSP);
                    if (sanPham != null)
                    {
                        decimal giaDau = sanPham.GiaDau ?? 0;
                        decimal soGiam = sanPham.SoGiam ?? 0;
                        decimal donGia = giaDau - (giaDau * soGiam / 100); // Tính giá sau giảm
                        tongTien += donGia * chiTiet.SoLuong;
                    }
                }
                tongTienList.Add(tongTien);
            }
            ViewBag.TongTienList = tongTienList;

            int pageSize = 5;
            int pageNumber = (page ?? 1);
            return View("Index", hoaddons.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Print(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var hoaDon = db.HoaDons
                .Include(h => h.ChiTietHoaDon)
                .FirstOrDefault(h => h.MaHD == id);

            if (hoaDon == null)
            {
                return HttpNotFound();
            }

            // Tính đơn giá và thành tiền, áp dụng giảm giá
            var chiTietList = hoaDon.ChiTietHoaDon.Select(ct =>
            {
                dynamic expando = new System.Dynamic.ExpandoObject();
                var sanPham = db.SanPhams.FirstOrDefault(sp => sp.MaSP == ct.MaSP);
                decimal giaDau = sanPham?.GiaDau ?? 0;
                decimal soGiam = sanPham?.SoGiam ?? 0;
                decimal donGia = giaDau - (giaDau * soGiam / 100); // Tính giá sau giảm
                expando.TenSP = sanPham?.TenSP ?? "Không rõ";
                expando.SoLuong = ct.SoLuong;
                expando.DonGia = donGia;
                expando.ThanhTien = donGia * ct.SoLuong;

                return expando;
            }).ToList();

            decimal tongTien = chiTietList.Sum(ct => (decimal)(ct.ThanhTien));

            ViewBag.ChiTietList = chiTietList;
            ViewBag.TongTien = tongTien;

            return View("Print", hoaDon);
        }

        [HttpPost]
        public ActionResult UpdateStatus(string id, int trangThai)
        {
            try
            {
                var hoaDon = db.HoaDons.FirstOrDefault(h => h.MaHD == id);
                if (hoaDon == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                hoaDon.TrangThai = trangThai;
                db.SaveChanges();

                if (trangThai == 2)
                {
                    string emailBody = $"<div style='color: black; font-family: Arial, sans-serif;'>" +
                                       $"<p>Chào {hoaDon.TenKH},</p>" +
                                       $"<p>Đơn hàng của bạn (Mã đơn: <strong>{hoaDon.MaHD}</strong>) đã được chuyển đến đơn vị vận chuyển.</p>" +
                                       "<p>Chi tiết đơn hàng:</p>" +
                                       "<table border='1' cellspacing='0' cellpadding='5' style='border-collapse: collapse; width: 100%; color: black;'>" +
                                       "<thead>" +
                                       "<tr>" +
                                       "<th style='text-align:left; color: black;'>Tên sản phẩm</th>" +
                                       "<th style='color: black;'>Số lượng</th>" +
                                       "<th style='color: black;'>Đơn giá</th>" +
                                       "<th style='color: black;'>Thành tiền</th>" +
                                       "</tr>" +
                                       "</thead><tbody>";

                    foreach (var item in hoaDon.ChiTietHoaDon)
                    {
                        decimal giaDau = item.SanPham.GiaDau ?? 0;
                        decimal soGiam = item.SanPham.SoGiam ?? 0;
                        decimal donGia = giaDau - (giaDau * soGiam / 100); // Tính giá sau giảm
                        decimal thanhTien = item.SoLuong * donGia;

                        emailBody += "<tr>" +
                                     $"<td style='color: black;'>{item.SanPham.TenSP}</td>" +
                                     $"<td style='text-align:center; color: black;'>{item.SoLuong}</td>" +
                                     $"<td style='text-align:right; color: black;'>{String.Format("{0:C0}", donGia)}</td>" +
                                     $"<td style='text-align:right; color: black;'>{String.Format("{0:C0}", thanhTien)}</td>" +
                                     "</tr>";
                    }

                    decimal tongTien = hoaDon.ChiTietHoaDon.Sum(c => c.SoLuong * ((c.SanPham.GiaDau ?? 0) - ((c.SanPham.GiaDau ?? 0) * (c.SanPham.SoGiam ?? 0) / 100)));
                    emailBody += $"<tr>" +
                        $"<td colspan='3' style='text-align:right; font-weight:bold;'>Tổng cộng:</td>" +
                        $"<td style='text-align:right; font-weight:bold;'>{tongTien:N0}₫</td>" +
                        "</tr>";
                    emailBody += "</tbody></table>" +
                                 "<p style='color: black;'>Dự kiến giao hàng trong vòng 2-5 ngày làm việc. Vui lòng giữ điện thoại ở trạng thái liên lạc để nhận hàng.</p>" +
                                 "<p style='color: black;'>Nếu có bất kỳ câu hỏi nào, vui lòng liên hệ:</p>" +
                                 "<ul>" +
                                 "<li>Hotline: <strong>0366 541 719</strong> (7:30 - 22:00)</li>" +
                                 "<li>Hỗ trợ: <strong>0366 541 718</strong> (8:00 - 21:00)</li>" +
                                 "</ul>" +
                                 "<p style='color: black;'><strong>Cảm ơn bạn đã mua sắm tại ELECTRONICS STORE!</strong></p>" +
                                 "<p style='color: black;'><em>Trân trọng,</em><br/>ELECTRONICS STORE</p>" +
                                 "</div>";

                    SendMail sendMail = new SendMail();
                    sendMail.SendMailFunction(hoaDon.Email, "Đơn hàng đang được vận chuyển", emailBody);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var hoaDon = db.HoaDons
                .Include(h => h.ChiTietHoaDon)
                .FirstOrDefault(h => h.MaHD == id);

            if (hoaDon == null)
            {
                return HttpNotFound();
            }

            var chiTietList = hoaDon.ChiTietHoaDon.Select(ct =>
            {
                dynamic expando = new System.Dynamic.ExpandoObject();
                var sanPham = db.SanPhams.FirstOrDefault(sp => sp.MaSP == ct.MaSP);
                decimal giaDau = sanPham?.GiaDau ?? 0;
                decimal soGiam = sanPham?.SoGiam ?? 0;
                decimal donGia = giaDau - (giaDau * soGiam / 100); // Tính giá sau giảm
                expando.TenSP = sanPham?.TenSP ?? "Không rõ";
                expando.SoLuong = ct.SoLuong;
                expando.DonGia = donGia;
                expando.ThanhTien = donGia * ct.SoLuong;

                return expando;
            }).ToList();

            decimal tongTien = chiTietList.Sum(ct => (decimal)(ct.ThanhTien));

            ViewBag.ChiTietList = chiTietList;
            ViewBag.TongTien = tongTien;

            return View(hoaDon);
        }

        public ActionResult Index(string status = "Active", string keyword = "", int? page = 1)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }

            int pageSize = 5;
            int pageNumber = (page ?? 1);
            var hoaDons = db.HoaDons.Include(hd => hd.ChiTietHoaDon).ToList();

            List<decimal> tongTienList = new List<decimal>();
            foreach (var hoaDon in hoaDons)
            {
                decimal tongTien = 0;
                foreach (var chiTiet in hoaDon.ChiTietHoaDon)
                {
                    var sanPham = db.SanPhams.Find(chiTiet.MaSP);
                    if (sanPham != null)
                    {
                        decimal giaDau = sanPham.GiaDau ?? 0;
                        decimal soGiam = sanPham.SoGiam ?? 0;
                        decimal donGia = giaDau - (giaDau * soGiam / 100); // Tính giá sau giảm
                        tongTien += donGia * chiTiet.SoLuong;
                    }
                }
                tongTienList.Add(tongTien);
            }

            ViewBag.TongTienList = tongTienList;
            return View(hoaDons.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Create()
        {
            ViewBag.SanPhams = db.SanPhams
                .Where(sp => sp.SoLuong > 0 && sp.Status == 1)
                .Select(sp => new SelectListItem
                {
                    Value = sp.MaSP,
                    Text = sp.TenSP
                }).ToList();

            ViewBag.SanPhamData = db.SanPhams
                .Where(sp => sp.SoLuong > 0 && sp.Status == 1)
                .Select(sp => new
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    Gia = sp.GiaDau ?? 0,
                    SoGiam = sp.SoGiam ?? 0,
                    GiaBan = (sp.GiaDau ?? 0) - ((sp.GiaDau ?? 0) * (sp.SoGiam ?? 0) / 100) // Tính giá sau giảm cho JavaScript
                }).ToList();

            string lastMaHD = db.HoaDons
                .OrderByDescending(h => h.MaHD)
                .Select(h => h.MaHD)
                .FirstOrDefault();
            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastMaHD) && lastMaHD.Length >= 5 && lastMaHD.StartsWith("HD"))
            {
                string numberPart = lastMaHD.Substring(2);
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }
            ViewBag.MaHD = "HD" + nextNumber.ToString("D3");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HoaDon hoaDon, string[] MaSPs, int[] SoLuongs)
        {
            // Kiểm tra danh sách sản phẩm
            if (MaSPs == null || MaSPs.Length == 0 || SoLuongs == null || SoLuongs.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng thêm ít nhất một sản phẩm vào hóa đơn.");
            }

            // Kiểm tra thông tin khách hàng
            if (string.IsNullOrWhiteSpace(hoaDon.TenKH))
            {
                ModelState.AddModelError("TenKH", "Tên khách hàng không được bỏ trống.");
            }
            if (string.IsNullOrWhiteSpace(hoaDon.SoDienThoai))
            {
                ModelState.AddModelError("SoDienThoai", "Số điện thoại không được bỏ trống.");
            }
            if (string.IsNullOrWhiteSpace(hoaDon.Email))
            {
                ModelState.AddModelError("Email", "Email không được bỏ trống.");
            }
            if (string.IsNullOrWhiteSpace(hoaDon.DiaChi))
            {
                ModelState.AddModelError("DiaChi", "Địa chỉ không được bỏ trống.");
            }

            // Kiểm tra định dạng email (tùy chọn)
            if (!string.IsNullOrWhiteSpace(hoaDon.Email) && !Regex.IsMatch(hoaDon.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ModelState.AddModelError("Email", "Email không hợp lệ.");
            }

            // Nếu có lỗi, gộp thông báo lỗi vào TempData
            if (!ModelState.IsValid)
            {
                // Gộp tất cả lỗi thành một chuỗi
                var errorMessages = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                TempData["ErrorMessage"] = string.Join("; ", errorMessages);

                // Trả về view với dữ liệu cần thiết
                ViewBag.SanPhams = db.SanPhams
                    .Where(sp => sp.SoLuong > 0 && sp.Status == 1)
                    .Select(sp => new SelectListItem
                    {
                        Value = sp.MaSP,
                        Text = sp.TenSP
                    }).ToList();

                ViewBag.SanPhamData = db.SanPhams
                    .Where(sp => sp.SoLuong > 0 && sp.Status == 1)
                    .Select(sp => new
                    {
                        MaSP = sp.MaSP,
                        TenSP = sp.TenSP,
                        Gia = sp.GiaDau ?? 0,
                        SoGiam = sp.SoGiam ?? 0,
                        GiaBan = (sp.GiaDau ?? 0) - ((sp.GiaDau ?? 0) * (sp.SoGiam ?? 0) / 100)
                    }).ToList();

                ViewBag.MaHD = hoaDon.MaHD; // Giữ mã hóa đơn đã sinh
                return View(hoaDon);
            }

            // Thiết lập thông tin hóa đơn
            string username = Session["Admin"] as string;
            hoaDon.NguoiTao = string.IsNullOrEmpty(username) ? "Unknown" : username;

            // Tạo mã hóa đơn mới
            string lastMaHD = db.HoaDons
                .OrderByDescending(h => h.MaHD)
                .Select(h => h.MaHD)
                .FirstOrDefault();
            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastMaHD) && lastMaHD.Length >= 5 && lastMaHD.StartsWith("HD"))
            {
                string numberPart = lastMaHD.Substring(2);
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }
            hoaDon.MaHD = "HD" + nextNumber.ToString("D3");
            hoaDon.NgayTao = DateTime.Now;
            hoaDon.TrangThai = 0;

            // Thêm hóa đơn vào database
            db.HoaDons.Add(hoaDon);
            db.SaveChanges();

            // Thêm chi tiết hóa đơn
            for (int i = 0; i < MaSPs.Length; i++)
            {
                int soLuong = SoLuongs[i];
                string maSP = MaSPs[i];

                if (soLuong > 0)
                {
                    string chiTietID = "CTHD_" + hoaDon.MaHD + "_" + (i + 1).ToString("D2");

                    var chiTiet = new ChiTietHoaDon
                    {
                        ID = chiTietID,
                        MaHD = hoaDon.MaHD,
                        MaSP = maSP,
                        SoLuong = soLuong
                    };
                    db.ChiTietHoaDons.Add(chiTiet);

                    var sanPham = db.SanPhams.FirstOrDefault(sp => sp.MaSP == maSP);
                    if (sanPham != null && sanPham.SoLuong >= soLuong)
                    {
                        sanPham.SoLuong -= soLuong;
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"Sản phẩm {sanPham?.TenSP ?? maSP} không đủ số lượng tồn kho.";
                        db.HoaDons.Remove(hoaDon); // Xóa hóa đơn đã thêm nếu có lỗi
                        db.SaveChanges();
                        return View(hoaDon);
                    }
                }
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult GetCustomerInfo(string soDienThoai, string email)
        {
            try
            {
                var hoaDon = db.HoaDons
                    .Where(h => (!string.IsNullOrEmpty(soDienThoai) && h.SoDienThoai == soDienThoai) ||
                                (!string.IsNullOrEmpty(email) && h.Email == email))
                    .OrderByDescending(h => h.NgayTao)
                    .FirstOrDefault();

                if (hoaDon != null)
                {
                    return Json(new
                    {
                        success = true,
                        data = new
                        {
                            TenKH = hoaDon.TenKH,
                            SoDienThoai = hoaDon.SoDienThoai,
                            Email = hoaDon.Email,
                            DiaChi = hoaDon.DiaChi
                        }
                    });
                }

                return Json(new { success = false });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        [HttpGet]
        public JsonResult GetProducts(int page = 1, int pageSize = 5)
        {
            try
            {
                var products = db.SanPhams
                    .Where(sp => sp.SoLuong > 0 && sp.Status == 1)
                    .Select(sp => new
                    {
                        MaSP = sp.MaSP,
                        TenSP = sp.TenSP,
                        GiaBan = (sp.GiaDau ?? 0) - ((sp.GiaDau ?? 0) * (sp.SoGiam ?? 0) / 100)
                    })
                    .OrderBy(sp => sp.TenSP)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var totalProducts = db.SanPhams
                    .Count(sp => sp.SoLuong > 0 && sp.Status == 1);

                return Json(new
                {
                    success = true,
                    data = products,
                    hasMore = page * pageSize < totalProducts
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}