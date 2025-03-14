using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using chuyende.Models;

namespace chuyende.Areas.Admin.Controllers
{
    public class ChucVusController : Controller
    {
        private QuanLyBanDienTuEntities1 db = new QuanLyBanDienTuEntities1();

        // GET: Admin/ChucVus
        public ActionResult Index()
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }
            return View(db.ChucVu.ToList());
        }

        // GET: Admin/ChucVus/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy mã chức vụ.";
                return RedirectToAction("Index");
            }
            ChucVu chucVu = db.ChucVu.Find(id);
            if (chucVu == null)
            {
                TempData["ErrorMessage"] = "Chức vụ không tồn tại.";
                return RedirectToAction("Index");
            }
            return View(chucVu);
        }

        // GET: Admin/ChucVus/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/ChucVus/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaCV,TenCV")] ChucVu chucVu)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.ChucVu.Add(chucVu);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Thêm chức vụ thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Thêm chức vụ không thành công.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            }
            return View(chucVu);
        }

        // GET: Admin/ChucVus/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy mã chức vụ.";
                return RedirectToAction("Index");
            }
            ChucVu chucVu = db.ChucVu.Find(id);
            if (chucVu == null)
            {
                TempData["ErrorMessage"] = "Chức vụ không tồn tại.";
                return RedirectToAction("Index");
            }
            return View(chucVu);
        }

        // POST: Admin/ChucVus/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaCV,TenCV")] ChucVu chucVu)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(chucVu).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật chức vụ thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Cập nhật chức vụ không thành công.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            }
            return View(chucVu);
        }

        // GET: Admin/ChucVus/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy mã chức vụ.";
                return RedirectToAction("Index");
            }
            ChucVu chucVu = db.ChucVu.Find(id);
            if (chucVu == null)
            {
                TempData["ErrorMessage"] = "Chức vụ không tồn tại.";
                return RedirectToAction("Index");
            }
            return View(chucVu);
        }

        // POST: Admin/ChucVus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                ChucVu chucVu = db.ChucVu.Find(id);
                if (chucVu == null)
                {
                    TempData["ErrorMessage"] = "Chức vụ không tồn tại.";
                    return RedirectToAction("Index");
                }
                db.ChucVu.Remove(chucVu);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Xóa chức vụ thành công!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Xóa chức vụ không thành công.";
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
