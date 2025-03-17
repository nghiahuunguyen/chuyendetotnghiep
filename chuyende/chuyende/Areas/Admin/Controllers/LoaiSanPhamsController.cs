using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using chuyende.Models;

namespace chuyende.Areas.Admin.Controllers
{
    public class LoaiSanPhamsController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        // Danh sách loại sản phẩm theo trạng thái (mặc định là đang hoạt động)
        public ActionResult Index(int status = 1)
        {
            var loaiSanPhams = db.LoaiSanPhams.Where(lsp => lsp.Status == status).ToList();
            return View(loaiSanPhams);
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
        public ActionResult Create([Bind(Include = "TenLoaiSP")] LoaiSanPham loaiSanPham)
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
            if (id == null) return RedirectToAction("Index");
            LoaiSanPham loaiSanPham = db.LoaiSanPhams.Find(id);
            if (loaiSanPham == null) return RedirectToAction("Index");
            return View(loaiSanPham);
        }

        // Xử lý chỉnh sửa loại sản phẩm hoặc chuyển vào thùng rác
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LoaiSanPham loaiSanPham, string actionType)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (actionType == "trash")
                    {
                        loaiSanPham.Status = 0;
                        db.Entry(loaiSanPham).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Trash");
                    }
                    else
                    {
                        db.Entry(loaiSanPham).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Cập nhật không thành công.";
                }
            }
            return View(loaiSanPham);
        }

        // Danh sách loại sản phẩm trong thùng rác
        public ActionResult Trash()
        {
            var deletedLoaiSanPhams = db.LoaiSanPhams.Where(lsp => lsp.Status == 0).ToList();
            return View(deletedLoaiSanPhams);
        }

        // Chuyển loại sản phẩm vào thùng rác
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoveToTrash(string id)
        {
            LoaiSanPham loaiSanPham = db.LoaiSanPhams.Find(id);
            if (loaiSanPham != null)
            {
                loaiSanPham.Status = 0;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // Khôi phục loại sản phẩm từ thùng rác
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Restore(string id)
        {
            LoaiSanPham loaiSanPham = db.LoaiSanPhams.Find(id);
            if (loaiSanPham != null)
            {
                loaiSanPham.Status = 1;
                db.SaveChanges();
            }
            return RedirectToAction("Trash");
        }

        // Xóa vĩnh viễn loại sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForever(string id)
        {
            LoaiSanPham loaiSanPham = db.LoaiSanPhams.Find(id);
            if (loaiSanPham != null)
            {
                db.LoaiSanPhams.Remove(loaiSanPham);
                db.SaveChanges();
            }
            return RedirectToAction("Trash");
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