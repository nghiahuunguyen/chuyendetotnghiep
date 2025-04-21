using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
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
                return RedirectToAction("Index"); // Nếu không nhập gì, hiển thị tất cả
            }

            // Lấy danh sách hóa đơn phù hợp với từ khóa
            var hoaddons = db.HoaDons
                .Include(hd => hd.ChiTietHoaDon) // Include ChiTietHoaDon to avoid lazy loading issues
                .Where(h => h.MaHD.Contains(keyword) || h.TenKH.Contains(keyword)
                            || h.SoDienThoai.Contains(keyword) || h.Email.Contains(keyword))
                .ToList();

            if (hoaddons == null || !hoaddons.Any())
            {
                TempData["ErrorMessage"] = "Không tìm thấy hóa đơn nào phù hợp.";
                return RedirectToAction("Index");
            }

            // Tính tổng tiền cho mỗi hóa đơn và lưu vào ViewBag
            List<decimal> tongTienList = new List<decimal>();
            foreach (var hoaDon in hoaddons)
            {
                decimal tongTien = 0;
                foreach (var chiTiet in hoaDon.ChiTietHoaDon)
                {
                    var sanPham = db.SanPhams.Find(chiTiet.MaSP);
                    if (sanPham != null)
                    {
                        tongTien += (sanPham.GiaDau ?? 0) * chiTiet.SoLuong;
                    }
                }
                tongTienList.Add(tongTien);
            }
            ViewBag.TongTienList = tongTienList; // Truyền danh sách tổng tiền vào View

            // Phân trang
            int pageSize = 5; // Số hóa đơn mỗi trang
            int pageNumber = (page ?? 1); // Trang hiện tại
            return View("Index", hoaddons.ToPagedList(pageNumber, pageSize)); // Trả về danh sách hóa đơn phân trang
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

            // Sử dụng ExpandoObject để tạo đối tượng động
            var chiTietList = hoaDon.ChiTietHoaDon.Select(ct =>
            {
                dynamic expando = new System.Dynamic.ExpandoObject();
                var sanPham = db.SanPhams.FirstOrDefault(sp => sp.MaSP == ct.MaSP);
                expando.TenSP = sanPham?.TenSP ?? "Không rõ";
                expando.SoLuong = ct.SoLuong;
                expando.DonGia = sanPham?.GiaDau ?? 0;
                expando.ThanhTien = (sanPham?.GiaDau ?? 0) * ct.SoLuong;

                return expando;
            }).ToList();

            // Tính tổng tiền, xử lý với kiểu decimal
            decimal tongTien = chiTietList.Sum(ct => (decimal)(ct.ThanhTien));

            // Truyền danh sách chi tiết và tổng tiền vào ViewBag
            ViewBag.ChiTietList = chiTietList;
            ViewBag.TongTien = tongTien;

            // Tương tự logic trong Details, có thể dùng lại ViewBag nếu cần
            // hoặc truyền model cụ thể nếu dùng ViewModel riêng cho in hóa đơn

            return View("Print", hoaDon); // View riêng cho in
        }


        // GET: Admin/HoaDons
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

                // Nếu trạng thái là "Đang vận chuyển" thì gửi email cho khách hàng
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
                        decimal donGia = giaDau - soGiam;
                        decimal thanhTien = item.SoLuong * donGia;

                        emailBody += "<tr>" +
                                     $"<td style='color: black;'>{item.SanPham.TenSP}</td>" +
                                     $"<td style='text-align:center; color: black;'>{item.SoLuong}</td>" +
                                     $"<td style='text-align:right; color: black;'>{String.Format("{0:C0}", donGia)}</td>" +
                                     $"<td style='text-align:right; color: black;'>{String.Format("{0:C0}", thanhTien)}</td>" +
                                     "</tr>";
                    }

                    decimal tongTien = hoaDon.ChiTietHoaDon.Sum(c => c.SoLuong * ((c.SanPham.GiaDau ?? 0) - (c.SanPham.SoGiam ?? 0)));

                    emailBody += "</tbody></table>" +
                                 $"<p style='color: black;'>Tổng tiền: <strong>{String.Format("{0:C0}", tongTien)}</strong></p>" +
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


        // GET: Admin/HoaDons/Details/HD001
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

            // Sử dụng ExpandoObject để tạo đối tượng động
            var chiTietList = hoaDon.ChiTietHoaDon.Select(ct =>
            {
                dynamic expando = new System.Dynamic.ExpandoObject();
                var sanPham = db.SanPhams.FirstOrDefault(sp => sp.MaSP == ct.MaSP);
                expando.TenSP = sanPham?.TenSP ?? "Không rõ";
                expando.SoLuong = ct.SoLuong;
                expando.DonGia = sanPham?.GiaDau ?? 0;
                expando.ThanhTien = (sanPham?.GiaDau ?? 0) * ct.SoLuong;

                return expando;
            }).ToList();

            // Tính tổng tiền, xử lý với kiểu decimal
            decimal tongTien = chiTietList.Sum(ct => (decimal)(ct.ThanhTien));

            // Truyền danh sách chi tiết và tổng tiền vào ViewBag
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

            int pageSize = 5; // Số hãng mỗi trang
            int pageNumber = (page ?? 1); // Nếu không có trang, mặc định trang 1
            var hoaDons = db.HoaDons.Include(hd => hd.ChiTietHoaDon).ToList();

            // Tính tổng tiền cho mỗi hóa đơn và lưu vào ViewBag
            List<decimal> tongTienList = new List<decimal>();
            foreach (var hoaDon in hoaDons)
            {
                decimal tongTien = 0;

                // Tính tổng tiền cho mỗi hóa đơn
                foreach (var chiTiet in hoaDon.ChiTietHoaDon)
                {
                    var sanPham = db.SanPhams.Find(chiTiet.MaSP);
                    if (sanPham != null)
                    {
                        tongTien += (sanPham.GiaDau ?? 0) * chiTiet.SoLuong;
                    }
                }

                tongTienList.Add(tongTien); // Lưu tổng tiền vào danh sách
            }

            ViewBag.TongTienList = tongTienList; // Truyền danh sách tổng tiền vào View

            return View(hoaDons.ToPagedList(pageNumber, pageSize)); // Truyền danh sách hóa đơn vào View
        }

        // GET: Admin/HoaDons/Create
        public ActionResult Create()
        {
            // Dữ liệu dùng cho dropdown
            ViewBag.SanPhams = db.SanPhams
                .Where(sp => sp.SoLuong > 0 && sp.Status == 1)
                .Select(sp => new SelectListItem
                {
                    Value = sp.MaSP,
                    Text = sp.TenSP
                }).ToList();

            // Dữ liệu dùng cho JavaScript
            ViewBag.SanPhamData = db.SanPhams
                .Where(sp => sp.SoLuong > 0 && sp.Status == 1)
                .Select(sp => new
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    Gia = sp.GiaDau ?? 0
                }).ToList();

            // Tạo MaHD tạm thời
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
            ViewBag.MaHD = "HD" + nextNumber.ToString("D3"); // Truyền MaHD tạm vào ViewBag

            return View();
        }

        // POST: Admin/HoaDons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HoaDon hoaDon, string[] MaSPs, int[] SoLuongs)
        {
            if (!ModelState.IsValid)
            {
                var sanPhams = db.SanPhams
                    .Where(sp => sp.SoLuong > 0 && sp.Status == 1)
                    .Select(sp => new
                    {
                        MaSP = sp.MaSP,
                        TenSP = sp.TenSP,
                        Gia = sp.GiaDau ?? 0
                    }).ToList();
                ViewBag.SanPhams = sanPhams;
                return View(hoaDon);
            }

            // Thêm thông tin người tạo
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

            db.HoaDons.Add(hoaDon);
            db.SaveChanges();  // Lưu hóa đơn để có MaHD

            // Lưu các ChiTietHoaDon cho các sản phẩm trong hóa đơn
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

                    // Tách biến ra để tránh lỗi LINQ to Entities
                    var sanPham = db.SanPhams.FirstOrDefault(sp => sp.MaSP == maSP);
                    if (sanPham != null && sanPham.SoLuong >= soLuong)
                    {
                        sanPham.SoLuong -= soLuong;
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Sản phẩm {sanPham?.TenSP ?? maSP} không đủ số lượng tồn kho.");
                        return View(hoaDon);
                    }
                }
            }

            db.SaveChanges();
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
    }
}
