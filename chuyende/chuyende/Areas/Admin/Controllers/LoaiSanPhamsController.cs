using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using chuyende.Models;
using PagedList;

namespace chuyende.Areas.Admin.Controllers
{
    public class LoaiSanPhamsController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        // Danh sách loại sản phẩm theo trạng thái (mặc định là đang hoạt động)
        public ActionResult Index(int status = 3, string keyword = "", int page = 1)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }
            int pageSize = 5; // Số lượng mục trên mỗi trang

            // Khởi tạo query cơ bản
            var loaiSanPhams = db.LoaiSanPhams.AsQueryable();

            

            // Lọc theo keyword (nếu có)
            if (!string.IsNullOrEmpty(keyword))
            {
                loaiSanPhams = loaiSanPhams.Where(lsp => lsp.TenLoaiSP.Contains(keyword));
            }

            // Lọc theo status
            switch (status)
            {
                case 1: // Active
                    loaiSanPhams = loaiSanPhams.Where(sp => sp.Status == 1);
                    break;
                case 2: // Unpublished
                    loaiSanPhams = loaiSanPhams.Where(sp => sp.Status == 2);
                    break;
                case 0: // Deleted
                    loaiSanPhams = loaiSanPhams.Where(sp => sp.Status == 0);
                    break;
                case 3: // Active + Unpublished
                    loaiSanPhams = loaiSanPhams.Where(sp => sp.Status == 1 || sp.Status == 2);
                    break;
            }

            // Phân trang
            var pagedLoaiSanPhams = loaiSanPhams.OrderBy(lsp => lsp.MaLoaiSP).ToPagedList(page, pageSize);

            return View(pagedLoaiSanPhams);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Publish(string id)
        {
            var loaisanPham = db.LoaiSanPhams.Find(id);
            if (loaisanPham != null)
            {
                loaisanPham.Status = 1; 
                db.SaveChanges();
                TempData["SuccessMessage"] = "Loại sản phẩm đã được xuất bản!";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Unpublish(string id)
        {
            var loaisanPham = db.LoaiSanPhams.Find(id);
            if (loaisanPham != null)
            {
                loaisanPham.Status = 2; // Không xuất bản
                db.SaveChanges();
                TempData["WarningMessage"] = "Loại sản phẩm đã được chuyển sang trạng thái không xuất bản.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ToggleStatus(string id)
        {
            var loaisanPham = db.LoaiSanPhams.Find(id);
            if (loaisanPham != null)
            {
                loaisanPham.Status = (loaisanPham.Status == 1) ? 2 : 1; // Chuyển đổi trạng thái
                db.SaveChanges();
                return Json(new { success = true, status =  loaisanPham.Status });
            }
            return Json(new { success = false });
        }

        // Hiển thị chi tiết loại sản phẩm
        public ActionResult Details(string id)
        {
            if (id == null) return RedirectToAction("Index");
            LoaiSanPham loaiSanPham = db.LoaiSanPhams.Find(id);
            if (loaiSanPham == null || loaiSanPham.Status == 0) return RedirectToAction("Index");
            return View(loaiSanPham);
        }

        // Hiển thị form tạo loại sản phẩm mới
        public ActionResult Create()
        {
            return View();
        }

        // Xử lý tạo loại sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TenLoaiSP,Link")] LoaiSanPham loaiSanPham)
        {
            if (ModelState.IsValid)
            {
                var maxMaLoaiSP = db.LoaiSanPhams.OrderByDescending(lsp => lsp.MaLoaiSP).Select(lsp => lsp.MaLoaiSP).FirstOrDefault();
                int newId = (maxMaLoaiSP != null) ? int.Parse(maxMaLoaiSP.Substring(3)) + 1 : 1;
                loaiSanPham.MaLoaiSP = "LSP" + newId.ToString("D3");
                loaiSanPham.Status = 1;
                db.LoaiSanPhams.Add(loaiSanPham);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(loaiSanPham);
        }
        // Hiển thị form chỉnh sửa loại sản phẩm
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy mã loại sản phẩm.";
                return RedirectToAction("Index");
            }

            LoaiSanPham loaiSanPham = db.LoaiSanPhams.Find(id);
            if (loaiSanPham == null)
            {
                TempData["ErrorMessage"] = "Loại sản phẩm không tồn tại.";
                return RedirectToAction("Index");
            }

            return View(loaiSanPham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaLoaiSP,TenLoaiSP,Link")] LoaiSanPham loaiSanPham)
        {
            if (ModelState.IsValid)
            {
                var existingLoaiSanPham = db.LoaiSanPhams.Find(loaiSanPham.MaLoaiSP);
                if (existingLoaiSanPham != null)
                {
                    existingLoaiSanPham.TenLoaiSP = loaiSanPham.TenLoaiSP;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Loại sản phẩm đã được cập nhật!";
                }
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            return View(loaiSanPham);
        }

        // Chuyển chức vụ vào thùng rác
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoveToTrash(string id)
        {
            LoaiSanPham loaiSanPham = db.LoaiSanPhams.Find(id);
            if (loaiSanPham != null)
            {
                loaiSanPham.Status = 0;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Loại sản phẩm đã được chuyển vào thùng rác.";
            }
            return RedirectToAction("Index");
        }

        // Danh sách loại sản phẩm trong thùng rác
        public ActionResult Trash()
        {
            var deletedLoaiSanPhams = db.LoaiSanPhams.Where(lsp => lsp.Status == 0).ToList();
            return View(deletedLoaiSanPhams);
        }

        // Khôi phục chức vụ từ thùng rác
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Restore(string id)
        {
            LoaiSanPham loaiSanPham = db.LoaiSanPhams.Find(id);
            if (loaiSanPham != null)
            {
                loaiSanPham.Status = 1;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Loại sản phẩm đã được khôi phục!";
            }
            return RedirectToAction("Index");
        }

        // Xóa vĩnh viễn một chức vụ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForever(string id)
        {
            var loaiSanPham = db.LoaiSanPhams.Find(id);
            if (loaiSanPham == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy loại sản phẩm cần xóa.";
                return RedirectToAction("Index", new { status = "Deleted" });
            }

            try
            {
                db.LoaiSanPhams.Remove(loaiSanPham);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Loại sản phẩm đã bị xóa vĩnh viễn!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Không thể xóa vì loại sản phẩm đang được sử dụng trong dữ liệu khác. Vui lòng kiểm tra lại!";
            }

            return RedirectToAction("Index", new { status = "Deleted" });
        }



        // Khôi phục tất cả chức vụ từ thùng rác
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RestoreAll()
        {
            var deletedLoaiSanPhams = db.LoaiSanPhams.Where(m => m.Status == 0).ToList();
            foreach (var chucVu in deletedLoaiSanPhams)
            {
                chucVu.Status = 1;
            }
            db.SaveChanges();
            TempData["SuccessMessage"] = "Tất cả loại sản phẩm đã được khôi phục!";
            return RedirectToAction("Index");
        }

        // Xóa tất cả chức vụ trong thùng rác vĩnh viễn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAllForever()
        {
            try
            {
                var deletedLoaiSanPhams = db.LoaiSanPhams.Where(l => l.Status == 0).ToList();
                db.LoaiSanPhams.RemoveRange(deletedLoaiSanPhams);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Tất cả loại sản phẩm đã bị xóa vĩnh viễn!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Không thể xóa do một số loại sản phẩm có ràng buộc dữ liệu. Vui lòng kiểm tra lại!";
            }

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