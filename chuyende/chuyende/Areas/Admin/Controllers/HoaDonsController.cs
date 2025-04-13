using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using chuyende.Helper;
using chuyende.Models;

namespace chuyende.Areas.Admin.Controllers
{
    public class HoaDonsController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

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
                    string emailBody = $"<p>Chào {hoaDon.TenKH},</p>" +
                                       $"<p>Đơn hàng của bạn (Mã đơn: <strong>{hoaDon.MaHD}</strong>) hiện đang được <strong>vận chuyển</strong>.</p>" +
                                       "<p>Chúng tôi sẽ giao hàng đến bạn trong thời gian sớm nhất và hãy để chú ý điện thoại của bạn.</p>" +
                                       "<p>Cảm ơn bạn đã mua sắm tại <strong>ELECTRONICS STORE</strong>.</p>";

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


        public ActionResult Index()
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }
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

            return View(hoaDons); // Truyền danh sách hóa đơn vào View
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
