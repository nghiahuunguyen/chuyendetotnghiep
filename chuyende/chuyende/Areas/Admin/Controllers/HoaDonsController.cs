using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using chuyende.Models;

namespace chuyende.Areas.Admin.Controllers
{
    public class HoaDonsController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        // GET: Admin/HoaDons
        public ActionResult Index()
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }
            return View(db.HoaDons.ToList());
        }

        // GET: Admin/HoaDons/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HoaDon hoaDon = db.HoaDons.Find(id);
            if (hoaDon == null)
            {
                return HttpNotFound();
            }
            return View(hoaDon);
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

            return View();
        }

        // POST: Admin/HoaDons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HoaDon hoaDon, string[] MaSPs, int[] SoLuongs)
        {
            if (!ModelState.IsValid)
            {
                // Nếu có lỗi, lấy lại danh sách sản phẩm như trên
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

            // Lấy tên người dùng từ session
            string username = Session["Admin"] as string; // Lấy từ Session["Admin"] thay vì User.Identity.Name

            // Kiểm tra nếu tên người dùng có trong session hay không
            if (string.IsNullOrEmpty(username))
            {
                hoaDon.NguoiTao = "Unknown (Not Logged In)";
                Debug.WriteLine("Không có tên người dùng trong session.");
            }
            else
            {
                // Tìm kiếm người dùng trong cơ sở dữ liệu
                var nhanVien = db.NhanViens.FirstOrDefault(nv => nv.TenDN == username);

                if (nhanVien != null)
                {
                    hoaDon.NguoiTao = nhanVien.TenNV;
                    Debug.WriteLine("Người tạo là: " + nhanVien.TenNV); // In ra tên nhân viên
                }
                else
                {
                    hoaDon.NguoiTao = "Unknown (User Not Found)";
                    Debug.WriteLine("Không tìm thấy người dùng: " + username); // Thông báo khi không tìm thấy người dùng
                }
            }



            // Tạo mã hóa đơn tự động dạng HD001, HD002, ...
            string lastMaHD = db.HoaDons
                .OrderByDescending(h => h.MaHD)
                .Select(h => h.MaHD)
                .FirstOrDefault();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastMaHD) && lastMaHD.Length >= 5 && lastMaHD.StartsWith("HD"))
            {
                string numberPart = lastMaHD.Substring(2); // Bỏ "HD"
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            hoaDon.MaHD = "HD" + nextNumber.ToString("D3");
            hoaDon.NgayTao = DateTime.Now;
            hoaDon.TrangThai = 0;

            db.HoaDons.Add(hoaDon);

            // Tạo chi tiết hóa đơn
            for (int i = 0; i < MaSPs.Length; i++)
            {
                if (SoLuongs[i] > 0)
                {
                    // Tìm mã CH cuối cùng (ID) trong ChiTietHoaDons
                    string lastChiTietID = db.ChiTietHoaDons
                        .OrderByDescending(ct => ct.ID)
                        .Select(ct => ct.ID)
                        .FirstOrDefault();

                    int nextCTNumber = 1;

                    if (!string.IsNullOrEmpty(lastChiTietID) && lastChiTietID.StartsWith("CH"))
                    {
                        string numberPart = lastChiTietID.Substring(2);
                        if (int.TryParse(numberPart, out int lastNumber))
                        {
                            nextCTNumber = lastNumber + 1;
                        }
                    }

                    // Tạo ChiTietHoaDon mới
                    string chiTietID = "CH" + nextCTNumber.ToString("D3");
                    var cthd = new ChiTietHoaDon
                    {
                        ID = chiTietID,
                        MaHD = hoaDon.MaHD,
                        MaSP = MaSPs[i],
                        SoLuong = SoLuongs[i]
                    };

                    db.ChiTietHoaDons.Add(cthd);
                }
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Admin/HoaDons/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HoaDon hoaDon = db.HoaDons.Find(id);
            if (hoaDon == null)
            {
                return HttpNotFound();
            }
            return View(hoaDon);
        }

        // POST: Admin/HoaDons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaHD,TenKH,SoDienThoai,Email,DiaChi,PhuongThucThanhToan,TrangThai,NguoiTao,NgayTao")] HoaDon hoaDon)
        {
            if (ModelState.IsValid)
            {
                db.Entry(hoaDon).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(hoaDon);
        }

        // GET: Admin/HoaDons/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HoaDon hoaDon = db.HoaDons.Find(id);
            if (hoaDon == null)
            {
                return HttpNotFound();
            }
            return View(hoaDon);
        }

        // POST: Admin/HoaDons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            HoaDon hoaDon = db.HoaDons.Find(id);
            db.HoaDons.Remove(hoaDon);
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
