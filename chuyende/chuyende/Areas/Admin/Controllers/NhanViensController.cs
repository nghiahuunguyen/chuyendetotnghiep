using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using chuyende.Models;
using OfficeOpenXml;
using PagedList;

namespace chuyende.Areas.Admin.Controllers
{
    public class NhanViensController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        public ActionResult Index(string status = "Active", int page = 1, int pageSize = 10)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }
            var nhanViens = db.NhanViens.AsQueryable();

            

            if (status == "Active")
                nhanViens = nhanViens.Where(nv => nv.Status == 1);
            else if (status == "Deleted")
                nhanViens = nhanViens.Where(nv => nv.Status == 0);

            nhanViens = nhanViens.OrderBy(nv => nv.MaNV); // Sắp xếp theo mã nhân viên

            return View(nhanViens.ToPagedList(page, pageSize));
        }


        public ActionResult Details(string id)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }
            if (id == null) return RedirectToAction("Index");
            NhanVien nhanVien = db.NhanViens.Find(id);
            if (nhanVien == null) return RedirectToAction("Index");
            return View(nhanVien);
        }

        public ActionResult Create()
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }
            ViewBag.MaCV = new SelectList(db.ChucVus.Where(cv => cv.Status == 1), "MaCV", "TenCV");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TenNV,SoDienThoai,Email,NgaySinh,GioiTinh,CCCD,DiaChi,TenDN,MatKhau,MaCV")] NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                // Lấy mã NV lớn nhất hiện tại (chỉ lấy mã đang hoạt động hoặc tất cả nếu cần)
                var lastNhanVien = db.NhanViens
                                     .OrderByDescending(nv => nv.MaNV)
                                     .FirstOrDefault();

                int newId = 1;
                if (lastNhanVien != null && lastNhanVien.MaNV.StartsWith("NV"))
                {
                    if (int.TryParse(lastNhanVien.MaNV.Substring(2), out int parsedId))
                    {
                        newId = parsedId + 1;
                    }
                }

                string newMaNV = $"NV{newId:D3}";

                // Đảm bảo không trùng mã
                while (db.NhanViens.Any(nv => nv.MaNV == newMaNV))
                {
                    newId++;
                    newMaNV = $"NV{newId:D3}";
                }

                nhanVien.MaNV = newMaNV;
                nhanVien.Status = 1;

                db.NhanViens.Add(nhanVien);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.MaCV = new SelectList(db.ChucVus.Where(cv => cv.Status == 1), "MaCV", "TenCV", nhanVien.MaCV);
            TempData["ErrorMessage"] = "Thêm nhân viên thất bại. Vui lòng kiểm tra lại dữ liệu.";
            return View(nhanVien);
        }


        public ActionResult Edit(string id)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }
            if (id == null) return RedirectToAction("Index");
            NhanVien nhanVien = db.NhanViens.Find(id);
            if (nhanVien == null) return RedirectToAction("Index");
            ViewBag.MaCV = new SelectList(db.ChucVus, "MaCV", "TenCV", nhanVien.MaCV);
            return View(nhanVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaNV,TenNV,SoDienThoai,Email,NgaySinh,GioiTinh,CCCD,DiaChi,TenDN,MatKhau,MaCV")] NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                var existingNhanVien = db.NhanViens.Find(nhanVien.MaNV);
                if (existingNhanVien != null)
                {
                    existingNhanVien.TenNV = nhanVien.TenNV;
                    existingNhanVien.SoDienThoai = nhanVien.SoDienThoai;
                    existingNhanVien.Email = nhanVien.Email;
                    existingNhanVien.NgaySinh = nhanVien.NgaySinh;
                    existingNhanVien.GioiTinh = nhanVien.GioiTinh;
                    existingNhanVien.CCCD = nhanVien.CCCD;
                    existingNhanVien.DiaChi = nhanVien.DiaChi;
                    existingNhanVien.TenDN = nhanVien.TenDN;
                    existingNhanVien.MatKhau = nhanVien.MatKhau;
                    existingNhanVien.MaCV = nhanVien.MaCV;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật nhân viên thành công!";
                }
                return RedirectToAction("Index");
            }
            ViewBag.MaCV = new SelectList(db.ChucVus, "MaCV", "TenCV", nhanVien.MaCV);
            return View(nhanVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoveToTrash(string id)
        {
            NhanVien nhanVien = db.NhanViens.Find(id);
            if (nhanVien != null)
            {
                nhanVien.Status = 0;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Nhân viên đã được chuyển vào thùng rác.";
            }
            return RedirectToAction("Index");
        }

        public ActionResult Trash()
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }
            var deletedNhanViens = db.NhanViens.Where(nv => nv.Status == 0).ToList();
            return View(deletedNhanViens);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Restore(string id)
        {
            NhanVien nhanVien = db.NhanViens.Find(id);
            if (nhanVien != null)
            {
                nhanVien.Status = 1;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Nhân viên đã được khôi phục!";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForever(string id)
        {
            NhanVien nhanVien = db.NhanViens.Find(id);
            if (nhanVien != null)
            {
                db.NhanViens.Remove(nhanVien);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Nhân viên đã bị xóa vĩnh viễn.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RestoreAll()
        {
            var deletedNhanViens = db.NhanViens.Where(nv => nv.Status == 0).ToList();
            foreach (var nhanVien in deletedNhanViens)
            {
                nhanVien.Status = 1; // Đặt lại trạng thái hoạt động
            }
            db.SaveChanges();
            TempData["SuccessMessage"] = "Tất cả nhân viên đã được khôi phục!";
            return RedirectToAction("Index"); // Chuyển hướng về danh sách nhân viên
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAllForever()
        {
            var deletedNhanViens = db.NhanViens.Where(nv => nv.Status == 0).ToList();
            if (deletedNhanViens.Any())
            {
                db.NhanViens.RemoveRange(deletedNhanViens); // Xóa tất cả nhân viên trong thùng rác
                db.SaveChanges();
                TempData["SuccessMessage"] = "Tất cả nhân viên đã bị xóa vĩnh viễn.";
            }
            return RedirectToAction("Index"); // Chuyển hướng về danh sách nhân viên
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExportNhanVienToExcel()
        {
            try
            {
                var nhanViens = db.NhanViens
                                 .Where(nv => nv.Status == 1)
                                 .OrderBy(nv => nv.MaNV)
                                 .ToList();

                using (var package = new ExcelPackage())
                {
                    var ws = package.Workbook.Worksheets.Add("NhanVien");

                    // Header row
                    ws.Cells["A1"].Value = "Mã NV";
                    ws.Cells["B1"].Value = "Tên NV";
                    ws.Cells["C1"].Value = "SĐT";
                    ws.Cells["D1"].Value = "Email";
                    ws.Cells["E1"].Value = "Ngày sinh";
                    ws.Cells["F1"].Value = "Giới tính";
                    ws.Cells["G1"].Value = "CCCD";
                    ws.Cells["H1"].Value = "Địa chỉ";
                    ws.Cells["I1"].Value = "Chức vụ";

                    // Data rows
                    int row = 2;
                    foreach (var nv in nhanViens)
                    {
                        ws.Cells[row, 1].Value = nv.MaNV;
                        ws.Cells[row, 2].Value = nv.TenNV;
                        ws.Cells[row, 3].Value = nv.SoDienThoai;
                        ws.Cells[row, 4].Value = nv.Email;
                        ws.Cells[row, 5].Value = nv.NgaySinh == DateTime.MinValue ? "" : nv.NgaySinh.ToString("dd/MM/yyyy");
                        ws.Cells[row, 6].Value = nv.GioiTinh ? "Nam" : "Nữ";
                        ws.Cells[row, 7].Value = nv.CCCD;
                        ws.Cells[row, 8].Value = nv.DiaChi;

                        var chucVu = db.ChucVus.Find(nv.MaCV);
                        ws.Cells[row, 9].Value = chucVu != null ? chucVu.TenCV : "";

                        row++;
                    }

                    // Formatting
                    ws.Cells["A1:I1"].Style.Font.Bold = true;
                    ws.Cells[1, 1, row - 1, 9].AutoFitColumns();
                    ws.Cells["E2:E" + row].Style.Numberformat.Format = "dd/mm/yyyy"; // Format date column

                    // Generate file
                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    string fileName = $"DanhSachNhanVien_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xuất Excel: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
