using System.Linq;
using System.Web.Mvc;
using chuyende.Models;
using System.Data.Entity;

namespace chuyende.Areas.Admin.Controllers
{
    public class HomeAdminController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        public ActionResult Index()
        {
            if (Session["Admin"] == null || Session["TenDN"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }

            var adminUsername = Session["TenDN"].ToString();
            var nhanVien = db.NhanViens.FirstOrDefault(nv => nv.TenDN == adminUsername);
            if (nhanVien == null)
            {
                TempData["ErrorMessage"] = $"Không tìm thấy nhân viên với TenDN = {adminUsername}";
                ViewBag.MaNV = null;
                System.Diagnostics.Debug.WriteLine($"Index: Không tìm thấy nhân viên với TenDN = {adminUsername}");
            }
            else
            {
                ViewBag.MaNV = nhanVien.MaNV;
                System.Diagnostics.Debug.WriteLine($"Index: MaNV = {nhanVien.MaNV}");
            }

            var model = new ThongKeAdmin
            {
                DonHangChoXuLy = db.HoaDons.Count(hd => hd.TrangThai == 1),
                DonHangDangVanChuyen = db.HoaDons.Count(hd => hd.TrangThai == 2),
                DonHangDaHoanThanh = db.HoaDons.Count(hd => hd.TrangThai == 3 || hd.TrangThai == 0),
                LienHeChuaXem = db.LienHes.Count(lh => lh.TrangThai == 0)
            };
            return View(model);
        }

        [HttpGet]
        public ActionResult DoiMatKhau(string id)
        {
            if (Session["Admin"] == null || Session["TenDN"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }

            string resolvedId = id;
            if (string.IsNullOrEmpty(resolvedId))
            {
                var adminUsername = Session["TenDN"].ToString();
                var nhanVienBySession = db.NhanViens.FirstOrDefault(nv => nv.TenDN == adminUsername);
                if (nhanVienBySession == null)
                {
                    TempData["ErrorMessage"] = $"Không tìm thấy nhân viên với TenDN = {adminUsername}";
                    System.Diagnostics.Debug.WriteLine($"DoiMatKhau GET: Không tìm thấy nhân viên với TenDN = {adminUsername}");
                    return RedirectToAction("Index");
                }
                resolvedId = nhanVienBySession.MaNV;
            }

            var nhanVien = db.NhanViens.Find(resolvedId);
            if (nhanVien == null)
            {
                TempData["ErrorMessage"] = $"Không tìm thấy nhân viên với MaNV = {resolvedId}";
                System.Diagnostics.Debug.WriteLine($"DoiMatKhau GET: Không tìm thấy nhân viên với MaNV = {resolvedId}");
                return RedirectToAction("Index");
            }

            ViewBag.MaNV = nhanVien.MaNV;
            System.Diagnostics.Debug.WriteLine($"DoiMatKhau GET: MaNV = {nhanVien.MaNV}");
            return View(nhanVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiMatKhau(NhanVien model, string MatKhau, string ConfirmMatKhau)
        {
            if (Session["Admin"] == null || Session["TenDN"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }

            // Kiểm tra và lấy MaNV nếu model.MaNV rỗng
            if (string.IsNullOrEmpty(model.MaNV))
            {
                var adminUsername = Session["TenDN"].ToString();
                var nhanVienBySession = db.NhanViens.FirstOrDefault(nv => nv.TenDN == adminUsername);
                if (nhanVienBySession == null)
                {
                    TempData["ErrorMessage"] = $"Không tìm thấy nhân viên với TenDN = {adminUsername}";
                    return RedirectToAction("Index");
                }
                model.MaNV = nhanVienBySession.MaNV;
            }

            ViewBag.MaNV = model.MaNV; // Gán ViewBag.MaNV cho layout

            if (!string.IsNullOrEmpty(MatKhau) && MatKhau != ConfirmMatKhau)
            {
                ModelState.AddModelError("ConfirmMatKhau", "Mật khẩu xác nhận không khớp.");
                return View(model);
            }

            if (!string.IsNullOrEmpty(MatKhau) && string.IsNullOrEmpty(ConfirmMatKhau))
            {
                ModelState.AddModelError("ConfirmMatKhau", "Vui lòng xác nhận mật khẩu.");
                return View(model);
            }

            var nhanVien = db.NhanViens.Find(model.MaNV);
            if (nhanVien == null)
            {
                TempData["ErrorMessage"] = $"Không tìm thấy nhân viên với MaNV = {model.MaNV}";
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrEmpty(MatKhau))
            {
                nhanVien.MatKhau = MatKhau; // Cân nhắc mã hóa mật khẩu
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công.";
                return RedirectToAction("Index");
            }
           
            return RedirectToAction("Index");
        }

        
    }
}