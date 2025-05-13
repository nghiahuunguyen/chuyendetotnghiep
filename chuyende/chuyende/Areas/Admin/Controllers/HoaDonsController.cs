using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using chuyende.Helper;
using chuyende.Models;
using Newtonsoft.Json;
using PagedList;

namespace chuyende.Areas.Admin.Controllers
{
    public class HoaDonsController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        public ActionResult Search(string keyword, int? page = 1)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }
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
                        decimal donGia = giaDau - (giaDau * soGiam / 100);
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
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }
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
                decimal donGia = giaDau - (giaDau * soGiam / 100);
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
                        decimal donGia = giaDau - (giaDau * soGiam / 100);
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

                if (trangThai == 3)
                {
                    string emailBody = $"<div style='color: black; font-family: Arial, sans-serif;'>" +
                                       $"<p>Chào {hoaDon.TenKH},</p>" +
                                       $"<p>Đơn hàng của bạn (Mã đơn: <strong>{hoaDon.MaHD}</strong>) đã hoàn thành.</p>" +
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
                        decimal donGia = giaDau - (giaDau * soGiam / 100);
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
                                 "<p style='color: black;'>Nếu có bất kỳ câu hỏi nào, vui lòng liên hệ:</p>" +
                                 "<ul>" +
                                 "<li>Hotline: <strong>0366 541 719</strong> (7:30 - 22:00)</li>" +
                                 "<li>Hỗ trợ: <strong>0366 541 718</strong> (8:00 - 21:00)</li>" +
                                 "</ul>" +
                                 "<p style='color: black;'><strong>Cảm ơn bạn đã mua sắm tại ELECTRONICS STORE!</strong></p>" +
                                 "<p style='color: black;'><em>Trân trọng,</em><br/>ELECTRONICS STORE</p>" +
                                 "</div>";

                    SendMail sendMail = new SendMail();
                    sendMail.SendMailFunction(hoaDon.Email, "Đơn hàng đã hoàn thành", emailBody);
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
                decimal donGia = giaDau - (giaDau * soGiam / 100);
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
            var allHoaDons = db.HoaDons.Include(hd => hd.ChiTietHoaDon).ToList();
            var pagedHoaDons = allHoaDons.ToPagedList(pageNumber, pageSize);

            // Lọc hóa đơn trên trang hiện tại
            var currentPageHoaDons = pagedHoaDons.ToList();

            var sanPhamData = db.SanPhams
                .Select(sp => new
                {
                    MaSP = sp.MaSP,
                    GiaBan = (sp.GiaDau ?? 0) - ((sp.GiaDau ?? 0) * (sp.SoGiam ?? 0) / 100)
                }).ToList();

            List<decimal> tongTienList = new List<decimal>();
            foreach (var hoaDon in currentPageHoaDons)
            {
                decimal tongTien = 0;
                foreach (var chiTiet in hoaDon.ChiTietHoaDon)
                {
                    var sanPham = sanPhamData.FirstOrDefault(sp => sp.MaSP == chiTiet.MaSP);
                    if (sanPham != null)
                    {
                        decimal donGia = sanPham.GiaBan;
                        decimal thanhTien = donGia * chiTiet.SoLuong;
                        tongTien += thanhTien;
                    }
                }
                tongTienList.Add(tongTien);
            }

            ViewBag.TongTienList = tongTienList;
            return View(pagedHoaDons);

        }

        public ActionResult Create()
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }
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
        public ActionResult Create(HoaDon hoaDon, string[] MaSPs, int[] SoLuongs, string PhuongThucThanhToan)
        {
            if (MaSPs == null || MaSPs.Length == 0 || SoLuongs == null || SoLuongs.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng thêm ít nhất một sản phẩm vào hóa đơn.");
            }

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

            if (!string.IsNullOrWhiteSpace(hoaDon.Email) && !Regex.IsMatch(hoaDon.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ModelState.AddModelError("Email", "Email không hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                TempData["ErrorMessage"] = string.Join("; ", errorMessages);

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

                ViewBag.MaHD = hoaDon.MaHD;
                return View(hoaDon);
            }

            // Nếu chọn tiền mặt
            if (PhuongThucThanhToan == "1")
            {
                string username = Session["Admin"] as string;
                hoaDon.NguoiTao = string.IsNullOrEmpty(username) ? "Unknown" : username;

                string lastMaHD = db.HoaDons
                    .OrderByDescending(h => h.MaHD)
                    .Select(h => h.MaHD)
                    .FirstOrDefault();
                int nextNumber = 1;
                if (!string.IsNullOrEmpty(lastMaHD) && lastMaHD.StartsWith("HD"))
                {
                    string numberPart = lastMaHD.Substring(2);
                    if (int.TryParse(numberPart, out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }
                hoaDon.MaHD = "HD" + nextNumber.ToString("D3");
                hoaDon.NgayTao = DateTime.Now;
                hoaDon.TrangThai = 0; // Chưa thanh toán
                hoaDon.PhuongThucThanhToan = 1; // Tiền mặt

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.HoaDons.Add(hoaDon);

                        for (int i = 0; i < MaSPs.Length; i++)
                        {
                            int soLuong = SoLuongs[i];
                            string maSP = MaSPs[i];

                            if (soLuong <= 0)
                            {
                                throw new Exception("Số lượng sản phẩm không hợp lệ.");
                            }

                            var sanPham = db.SanPhams.FirstOrDefault(sp => sp.MaSP == maSP);
                            if (sanPham == null)
                            {
                                throw new Exception($"Sản phẩm {maSP} không tồn tại.");
                            }
                            if (sanPham.SoLuong < soLuong)
                            {
                                throw new Exception($"Sản phẩm {sanPham.TenSP} không đủ số lượng tồn kho.");
                            }

                            string chiTietID = $"CTHD_{hoaDon.MaHD}_{(i + 1):D2}";
                            var chiTiet = new ChiTietHoaDon
                            {
                                ID = chiTietID,
                                MaHD = hoaDon.MaHD,
                                MaSP = maSP,
                                SoLuong = soLuong
                            };
                            db.ChiTietHoaDons.Add(chiTiet);
                            sanPham.SoLuong -= soLuong;
                        }

                        db.SaveChanges();
                        transaction.Commit();
                        TempData["SuccessMessage"] = "Hóa đơn đã được tạo thành công!";
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        TempData["ErrorMessage"] = $"Lỗi khi tạo hóa đơn: {ex.Message}";
                        System.Diagnostics.Debug.WriteLine($"Lỗi khi tạo hóa đơn: {ex.Message}, StackTrace: {ex.StackTrace}");
                        if (ex.InnerException != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                        }

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

                        ViewBag.MaHD = hoaDon.MaHD;
                        return View(hoaDon);
                    }
                }
            }

            // Nếu chọn chuyển khoản, logic được xử lý qua JavaScript
            return View(hoaDon);
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
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy thông tin khách hàng: {ex.Message}, StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
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
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy danh sách sản phẩm: {ex.Message}, StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<JsonResult> InitiatePayment(decimal amount, string tenKH, string soDienThoai, string email, string diaChi, string itemsJson)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"InitiatePayment called with: amount={amount}, tenKH={tenKH}, soDienThoai={soDienThoai}, email={email}, diaChi={diaChi}, itemsJson={itemsJson}");

                if (amount <= 0)
                {
                    return Json(new { success = false, message = "Số tiền không hợp lệ." });
                }

                List<dynamic> items = JsonConvert.DeserializeObject<List<dynamic>>(itemsJson);
                if (items == null || !items.Any())
                {
                    return Json(new { success = false, message = "Danh sách sản phẩm trống." });
                }

                if (string.IsNullOrWhiteSpace(tenKH))
                {
                    return Json(new { success = false, message = "Tên khách hàng không được để trống." });
                }
                if (string.IsNullOrWhiteSpace(soDienThoai))
                {
                    return Json(new { success = false, message = "Số điện thoại không được để trống." });
                }
                if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    return Json(new { success = false, message = "Email không hợp lệ." });
                }
                if (string.IsNullOrWhiteSpace(diaChi))
                {
                    return Json(new { success = false, message = "Địa chỉ không được để trống." });
                }

                foreach (var item in items)
                {
                    string maSP = item.MaSP?.ToString();
                    int soLuong = (int)(item.SoLuong ?? 0);
                    if (string.IsNullOrEmpty(maSP) || soLuong <= 0)
                    {
                        return Json(new { success = false, message = "Thông tin sản phẩm không hợp lệ." });
                    }
                }

                string lastMaHD = db.HoaDons.OrderByDescending(h => h.MaHD).Select(h => h.MaHD).FirstOrDefault();
                int nextNumber = 1;
                if (!string.IsNullOrEmpty(lastMaHD) && lastMaHD.StartsWith("HD"))
                {
                    string numberPart = lastMaHD.Substring(2);
                    if (int.TryParse(numberPart, out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }
                string tempMaHD = "HD" + nextNumber.ToString("D3");

                Session["OrderItems"] = items;
                Session["CustomerInfo"] = new { TenKH = tenKH, SoDienThoai = soDienThoai, Email = email, DiaChi = diaChi };
                Session["TempMaHD"] = tempMaHD;
                Session["PaymentAmount"] = amount;

                // Tạo orderCode một lần duy nhất
                var strOrderCode = DateTime.UtcNow.ToString("MMddHHmmss");
                System.Diagnostics.Debug.WriteLine($"Generated orderCode: {strOrderCode}");
                Session["OrderCode"] = strOrderCode;

                var result = await GetPaymentRequest(amount, tempMaHD, strOrderCode);
                if (result == null)
                {
                    return Json(new { success = false, message = "Không thể tạo yêu cầu thanh toán: Kết quả từ GetPaymentRequest là null." });
                }

                System.Diagnostics.Debug.WriteLine($"GetPaymentRequest result: {JsonConvert.SerializeObject(result)}");

                var resultData = result.Data as dynamic;
                if (resultData == null)
                {
                    return Json(new { success = false, message = "Dữ liệu trả về từ GetPaymentRequest không hợp lệ." });
                }

                if (resultData.success)
                {
                    return Json(new { success = true, checkoutUrl = resultData.checkoutUrl });
                }
                else
                {
                    return Json(new { success = false, message = resultData.message ?? "Lỗi không xác định từ GetPaymentRequest." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi trong InitiatePayment: {ex.Message}, StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return Json(new { success = false, message = $"Lỗi khi tạo yêu cầu thanh toán: {ex.Message}" });
            }
        }

        public async Task<JsonResult> GetPaymentRequest(decimal amount, string maHD, string strOrderCode)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetPaymentRequest called with: amount={amount}, maHD={maHD}, orderCode={strOrderCode}");

                int orderCode = int.Parse(strOrderCode);
                string clientId = "c94459b0-2ac0-4e3e-9ca1-a7b7f1be4c4e";
                string clientKey = "b3e95017-fd00-442c-b77b-180bf90503d4";
                string description = $"Thanh toán hóa đơn {maHD}";
                string cancelUrl = Url.Action("PaymentCancel", "HoaDons", null, Request.Url.Scheme);
                string returnUrl = Url.Action("PaymentSuccess", "HoaDons", null, Request.Url.Scheme);

                string rawSignature = $"amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";
                string signature = GenerateSignature(rawSignature);

                System.Diagnostics.Debug.WriteLine($"Signature generated: {signature}");

                var requestBody = new
                {
                    orderCode = orderCode,
                    amount = amount,
                    description = description,
                    cancelUrl = cancelUrl,
                    returnUrl = returnUrl,
                    signature = signature
                };

                System.Diagnostics.Debug.WriteLine($"Request body: {JsonConvert.SerializeObject(requestBody)}");

                string json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-client-id", clientId);
                    client.DefaultRequestHeaders.Add("x-api-key", clientKey);

                    var response = await client.PostAsync("https://api-merchant.payos.vn/v2/payment-requests", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"PayOS API response: {responseString}");
                        var responseData = JsonConvert.DeserializeObject<dynamic>(responseString);
                        string checkoutUrl = responseData["data"]["checkoutUrl"]?.ToString();
                        if (string.IsNullOrEmpty(checkoutUrl))
                        {
                            return Json(new { success = false, message = "Checkout URL không tồn tại trong phản hồi từ PayOS." });
                        }
                        return Json(new { success = true, checkoutUrl = checkoutUrl });
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"PayOS API error: {error}");
                        return Json(new { success = false, message = $"API Error: {error}" });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi trong GetPaymentRequest: {ex.Message}, StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return Json(new { success = false, message = $"Lỗi khi gọi PayOS API: {ex.Message}" });
            }
        }

        public async Task<JsonResult> GetPaymentRequest(decimal amount, string maHD)
        {
            try
            {
                // Log thông tin đầu vào
                System.Diagnostics.Debug.WriteLine($"GetPaymentRequest called with: amount={amount}, maHD={maHD}");

                // Các thông tin cần thiết
                var strOrderCode = DateTime.UtcNow.ToString("MMddHHmmss");
                int orderCode = int.Parse(strOrderCode);
                string clientId = "c94459b0-2ac0-4e3e-9ca1-a7b7f1be4c4e";
                string clientKey = "b3e95017-fd00-442c-b77b-180bf90503d4";
                string description = $"Thanh toán hóa đơn {maHD}";
                string cancelUrl = Url.Action("PaymentCancel", "HoaDons", null, Request.Url.Scheme);
                string returnUrl = Url.Action("PaymentSuccess", "HoaDons", null, Request.Url.Scheme);

                // Tạo chữ ký
                string rawSignature = $"amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";
                string signature = GenerateSignature(rawSignature);

                // Log chữ ký
                System.Diagnostics.Debug.WriteLine($"Signature generated: {signature}");

                // Tạo request body
                var requestBody = new
                {
                    orderCode = orderCode,
                    amount = amount,
                    description = description,
                    cancelUrl = cancelUrl,
                    returnUrl = returnUrl,
                    signature = signature
                };

                // Log request body
                System.Diagnostics.Debug.WriteLine($"Request body: {JsonConvert.SerializeObject(requestBody)}");

                // Gửi request đến PayOS
                string json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-client-id", clientId);
                    client.DefaultRequestHeaders.Add("x-api-key", clientKey);

                    var response = await client.PostAsync("https://api-merchant.payos.vn/v2/payment-requests", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"PayOS API response: {responseString}");
                        var responseData = JsonConvert.DeserializeObject<dynamic>(responseString);
                        string checkoutUrl = responseData["data"]["checkoutUrl"]?.ToString();
                        if (string.IsNullOrEmpty(checkoutUrl))
                        {
                            return Json(new { success = false, message = "Checkout URL không tồn tại trong phản hồi từ PayOS." });
                        }
                        return Json(new { success = true, checkoutUrl = checkoutUrl });
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"PayOS API error: {error}");
                        return Json(new { success = false, message = $"API Error: {error}" });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi trong GetPaymentRequest: {ex.Message}, StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return Json(new { success = false, message = $"Lỗi khi gọi PayOS API: {ex.Message}" });
            }
        }

        private string GenerateSignature(string rawData)
        {
            string checksumKey = "22d9610dc5591bb9a042a45cde8663685fe878c6c0e562bec44fc30d3244d469";
            byte[] keyBytes = Encoding.UTF8.GetBytes(checksumKey);
            byte[] dataBytes = Encoding.UTF8.GetBytes(rawData);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hash = hmac.ComputeHash(dataBytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmPayment()
        {
            try
            {
                // Lấy orderCode từ Session
                string orderCode = Session["OrderCode"] as string;
                if (string.IsNullOrEmpty(orderCode))
                {
                    System.Diagnostics.Debug.WriteLine("Error: OrderCode not found in Session.");
                    return Json(new { success = false, message = "Không tìm thấy thông tin thanh toán trong Session." });
                }

                // Kiểm tra trạng thái thanh toán qua API PayOS
                bool isPaymentSuccessful = await VerifyPaymentStatus(orderCode);
                if (!isPaymentSuccessful)
                {
                    System.Diagnostics.Debug.WriteLine($"Payment verification failed for orderCode: {orderCode}");
                    return Json(new { success = false, message = "Thanh toán không thành công hoặc không tìm thấy giao dịch." });
                }

                // Lấy thông tin từ Session
                var items = Session["OrderItems"] as List<dynamic>;
                var customerInfo = Session["CustomerInfo"] as dynamic;
                var tempMaHD = Session["TempMaHD"] as string;
                var paymentAmount = Session["PaymentAmount"] as decimal?;

                // Log thông tin để debug
                System.Diagnostics.Debug.WriteLine($"OrderCode: {orderCode}");
                System.Diagnostics.Debug.WriteLine($"OrderItems: {JsonConvert.SerializeObject(items)}");
                System.Diagnostics.Debug.WriteLine($"CustomerInfo: {JsonConvert.SerializeObject(customerInfo)}");
                System.Diagnostics.Debug.WriteLine($"TempMaHD: {tempMaHD}");
                System.Diagnostics.Debug.WriteLine($"PaymentAmount: {paymentAmount}");

                // Kiểm tra dữ liệu Session
                if (items == null || !items.Any())
                {
                    return Json(new { success = false, message = "Danh sách sản phẩm trống." });
                }
                if (customerInfo == null)
                {
                    return Json(new { success = false, message = "Thông tin khách hàng không hợp lệ." });
                }
                if (string.IsNullOrEmpty(tempMaHD))
                {
                    return Json(new { success = false, message = "Mã hóa đơn tạm thời không hợp lệ." });
                }
                if (!paymentAmount.HasValue || paymentAmount <= 0)
                {
                    return Json(new { success = false, message = "Số tiền thanh toán không hợp lệ." });
                }

                // Kiểm tra hóa đơn trùng
                var existingHoaDon = db.HoaDons.FirstOrDefault(h => h.MaHD == tempMaHD);
                if (existingHoaDon != null)
                {
                    return Json(new { success = false, message = "Hóa đơn đã được xử lý trước đó." });
                }

                // Tạo hóa đơn
                var hoaDon = new HoaDon
                {
                    MaHD = tempMaHD,
                    TenKH = customerInfo.TenKH,
                    SoDienThoai = customerInfo.SoDienThoai,
                    Email = customerInfo.Email,
                    DiaChi = customerInfo.DiaChi,
                    NgayTao = DateTime.Now,
                    TrangThai = 0, // Đã thanh toán
                    PhuongThucThanhToan = 2, // Chuyển khoản
                    NguoiTao = Session["Admin"] as string ?? "System"
                };

                // Bắt đầu transaction
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.HoaDons.Add(hoaDon);

                        // Tạo chi tiết hóa đơn
                        int index = 1;
                        foreach (var item in items)
                        {
                            string maSP = item.MaSP?.ToString();
                            int soLuong = (int)(item.SoLuong ?? 0);

                            if (string.IsNullOrEmpty(maSP) || soLuong <= 0)
                            {
                                throw new Exception("Thông tin sản phẩm không hợp lệ.");
                            }

                            var sanPham = db.SanPhams.FirstOrDefault(sp => sp.MaSP == maSP);
                            if (sanPham == null)
                            {
                                throw new Exception($"Sản phẩm {maSP} không tồn tại.");
                            }
                            if (sanPham.SoLuong < soLuong)
                            {
                                throw new Exception($"Sản phẩm {sanPham.TenSP} không đủ số lượng tồn kho.");
                            }

                            string chiTietID = $"CTHD_{tempMaHD}_{index:D2}";
                            var chiTiet = new ChiTietHoaDon
                            {
                                ID = chiTietID,
                                MaHD = tempMaHD,
                                MaSP = maSP,
                                SoLuong = soLuong
                            };
                            db.ChiTietHoaDons.Add(chiTiet);
                            sanPham.SoLuong -= soLuong;
                            index++;
                        }

                        db.SaveChanges();
                        transaction.Commit(); // Xác nhận transaction

                        // Xóa Session
                        Session["OrderItems"] = null;
                        Session["CustomerInfo"] = null;
                        Session["TempMaHD"] = null;
                        Session["OrderCode"] = null;
                        Session["PaymentAmount"] = null;

                        return Json(new { success = true, message = "Hóa đơn đã được lưu thành công." });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine($"Lỗi khi lưu hóa đơn: {ex.Message}, StackTrace: {ex.StackTrace}");
                        if (ex.InnerException != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                        }
                        return Json(new { success = false, message = $"Lỗi khi lưu hóa đơn: {ex.Message}" });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi chung trong ConfirmPayment: {ex.Message}, StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return Json(new { success = false, message = $"Lỗi khi xử lý thanh toán: {ex.Message}" });
            }
        }

        [HttpGet]
        public ActionResult PaymentSuccess(string orderCode)
        {
            TempData["SuccessMessage"] = "Thanh toán thành công! Vui lòng xác nhận để lưu hóa đơn.";
            return RedirectToAction("Create");
        }

        private async Task<bool> VerifyPaymentStatus(string orderCode)
        {
            try
            {
                string clientId = "c94459b0-2ac0-4e3e-9ca1-a7b7f1be4c4e";
                string clientKey = "b3e95017-fd00-442c-b77b-180bf90503d4";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-client-id", clientId);
                    client.DefaultRequestHeaders.Add("x-api-key", clientKey);

                    var response = await client.GetAsync($"https://api-merchant.payos.vn/v2/payment-requests/{orderCode}");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"PayOS Verify response: {responseString}");
                        var responseData = JsonConvert.DeserializeObject<dynamic>(responseString);
                        string status = responseData["data"]["status"]?.ToString();
                        return status == "PAID"; // Trạng thái thanh toán thành công
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Lỗi khi kiểm tra trạng thái PayOS: {await response.Content.ReadAsStringAsync()}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi xác nhận trạng thái PayOS: {ex.Message}, StackTrace: {ex.StackTrace}");
                return false;
            }
        }
        public ActionResult PaymentCancel(string orderCode, string status)
        {
            TempData["ErrorMessage"] = "Thanh toán bị hủy.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult Payment(string checkoutData)
        {
            ViewBag.CheckoutData = checkoutData;
            return View();
        }
    }
}