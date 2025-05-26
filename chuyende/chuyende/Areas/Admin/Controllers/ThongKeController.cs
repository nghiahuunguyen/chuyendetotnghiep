using chuyende.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace chuyende.Areas.Admin.Controllers
{
    public class ThongKeController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        // GET: Admin/ThongKe
        public ActionResult Index(string loai = "ngay")
        {
            if (Session["Admin"] == null)
                return RedirectToAction("Index", "DangNhap");

            var loaiThongKe = new List<SelectListItem>
    {
        new SelectListItem { Text = "Theo ngày", Value = "ngay" },
        new SelectListItem { Text = "Theo tháng", Value = "thang" },
        new SelectListItem { Text = "Theo năm", Value = "nam" }
    };

            ViewBag.LoaiThongKe = new SelectList(loaiThongKe, "Value", "Text", loai);

            var viewModel = new ThongKeTongHopViewModel
            {
                DoanhThu = LayThongKe(loai),
                SanPhamBanChay = LaySanPhamBanChay(loai)
            };

            return View(viewModel);
        }

        private List<SanPhamBanChay> LaySanPhamBanChay(string loai)
        {
            var query = db.ChiTietHoaDons
                .Where(ct => ct.HoaDon.TrangThai == 0 || ct.HoaDon.TrangThai == 3);

            switch (loai)
            {
                case "thang":
                    return query
                        .GroupBy(ct => new
                        {
                            ct.SanPham.MaSP,
                            ct.SanPham.TenSP,
                            ct.SanPham.HinhAnh,
                            ct.HoaDon.NgayTao.Year,
                            ct.HoaDon.NgayTao.Month
                        })
                        .Select(g => new SanPhamBanChay
                        {
                            MaSP = g.Key.MaSP,
                            TenSP = g.Key.TenSP,
                            HinhAnh = g.Key.HinhAnh,
                            Nam = g.Key.Year,
                            Thang = g.Key.Month,
                            TongSoLuong = g.Sum(x => x.SoLuong)
                        })
                        .OrderByDescending(x => x.TongSoLuong)
                        .ToList();

                case "nam":
                    return query
                        .GroupBy(ct => new
                        {
                            ct.SanPham.MaSP,
                            ct.SanPham.TenSP,
                            ct.SanPham.HinhAnh,
                            ct.HoaDon.NgayTao.Year
                        })
                        .Select(g => new SanPhamBanChay
                        {
                            MaSP = g.Key.MaSP,
                            TenSP = g.Key.TenSP,
                            HinhAnh = g.Key.HinhAnh,
                            Nam = g.Key.Year,
                            TongSoLuong = g.Sum(x => x.SoLuong)
                        })
                        .OrderByDescending(x => x.TongSoLuong)
                        .ToList();

                default: // ngay
                    return query
                        .GroupBy(ct => new
                        {
                            ct.SanPham.MaSP,
                            ct.SanPham.TenSP,
                            ct.SanPham.HinhAnh,
                            Ngay = DbFunctions.TruncateTime(ct.HoaDon.NgayTao)
                        })
                        .Select(g => new SanPhamBanChay
                        {
                            MaSP = g.Key.MaSP,
                            TenSP = g.Key.TenSP,
                            HinhAnh = g.Key.HinhAnh,
                            Ngay = g.Key.Ngay.Value.Day,
                            Thang = g.Key.Ngay.Value.Month,
                            Nam = g.Key.Ngay.Value.Year,
                            TongSoLuong = g.Sum(x => x.SoLuong)
                        })
                        .OrderByDescending(x => x.TongSoLuong)
                        .ToList();
            }
        }


        private List<ThongKeDoanhThu> LayThongKe(string loai)
        {
            var query = db.ChiTietHoaDons
                .Where(ct => ct.HoaDon.TrangThai == 0 || ct.HoaDon.TrangThai == 3);

            switch (loai)
            {
                case "thang":
                    return query
                        .GroupBy(ct => new { ct.HoaDon.NgayTao.Year, ct.HoaDon.NgayTao.Month })
                        .Select(g => new ThongKeDoanhThu
                        {
                            Nam = g.Key.Year,
                            Thang = g.Key.Month,
                            SoLuong = g.Sum(x => x.SoLuong),
                            TongTien = g.Sum(x => x.SoLuong * ((x.SanPham.GiaDau ?? 0) - ((x.SanPham.GiaDau ?? 0) * (x.SanPham.SoGiam ?? 0) / 100)))
                        }).ToList();

                case "nam":
                    return query
                        .GroupBy(ct => ct.HoaDon.NgayTao.Year)
                        .Select(g => new ThongKeDoanhThu
                        {
                            Nam = g.Key,
                            SoLuong = g.Sum(x => x.SoLuong),
                            TongTien = g.Sum(x => x.SoLuong * ((x.SanPham.GiaDau ?? 0) - ((x.SanPham.GiaDau ?? 0) * (x.SanPham.SoGiam ?? 0) / 100)))
                        }).ToList();

                default: // "ngay"
                    return query
                        .GroupBy(ct => DbFunctions.TruncateTime(ct.HoaDon.NgayTao))
                        .Select(g => new ThongKeDoanhThu
                        {
                            Ngay = g.Key.Value.Day,
                            Thang = g.Key.Value.Month,
                            Nam = g.Key.Value.Year,
                            SoLuong = g.Sum(x => x.SoLuong),
                            TongTien = g.Sum(x => x.SoLuong * ((x.SanPham.GiaDau ?? 0) - ((x.SanPham.GiaDau ?? 0) * (x.SanPham.SoGiam ?? 0) / 100)))
                        }).ToList();
            }
        }

        public ActionResult ExportToExcel(string loai = "ngay")
        {
            var data = LayThongKe(loai);


            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("DoanhThu");

                ws.Cells["A1"].Value = "Thời gian";
                ws.Cells["B1"].Value = "Số lượng đơn hàng";
                ws.Cells["C1"].Value = "Tổng tiền";

                int row = 2;
                foreach (var item in data)
                {
                    string time;
                    if (loai == "thang")
                        time = $"Tháng {item.Thang}/{item.Nam}";
                    else if (loai == "nam")
                        time = $"Năm {item.Nam}";
                    else
                        time = $"{item.Ngay}/{item.Thang}/{item.Nam}";

                    ws.Cells[row, 1].Value = time;
                    ws.Cells[row, 2].Value = item.SoLuong;
                    ws.Cells[row, 3].Value = item.TongTien;
                    row++;
                }

                ws.Cells[1, 1, row - 1, 3].AutoFitColumns();
                ws.Cells["A1:C1"].Style.Font.Bold = true;
                // Format số tiền (không để dính lỗi hiển thị #####)
                ws.Cells["C2:C" + row].Style.Numberformat.Format = "#,##0"; // hoặc "#,##0 ₫" nếu muốn có đơn vị
                ws.Column(3).AutoFit(); // Cột C là cột Tổng tiền, tự động co giãn chiều rộng


                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"ThongKe_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }
}
