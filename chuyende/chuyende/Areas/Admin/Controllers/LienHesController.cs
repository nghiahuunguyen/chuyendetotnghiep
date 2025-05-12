using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using chuyende.Models;
using PagedList;

namespace chuyende.Areas.Admin.Controllers
{
    public class LienHesController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        // GET: Admin/LienHes
        public ActionResult Index(int? page = 1)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }
            int pageSize = 5;
            int pageNumber = (page ?? 1);

            var lienHes = db.LienHes.OrderByDescending(l => l.NgayGui).ToPagedList(pageNumber, pageSize);

            return View(lienHes);
        }


        public ActionResult ToggleTrangThai(int id)
        {
            var lienHe = db.LienHes.Find(id);
            if (lienHe != null && lienHe.TrangThai == 0)
            {
                lienHe.TrangThai = 1;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }


        public ActionResult Details(int? id)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }
            if (id == null)
                return RedirectToAction("Index");
            LienHe lienHe = db.LienHes.Find(id);
            if (lienHe == null)
                return RedirectToAction("Index");
            return View(lienHe);
        }

        public ActionResult Compose(int id)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "DangNhap");
            }
            var lienHe = db.LienHes.Find(id);
            if (lienHe == null)
            {
                return HttpNotFound();
            }

            ViewBag.MaLH = lienHe.MaLH;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult SendEmail(int maLH, string subject, string body, HttpPostedFileBase file)
        {
            var lienHe = db.LienHes.Find(maLH);
            if (lienHe == null)
            {
                return HttpNotFound();
            }

            string imageUrl = null;

            if (file != null && file.ContentLength > 0)
            {
                // Lưu file vào thư mục /Uploads
                string fileName = System.IO.Path.GetFileName(file.FileName);
                string path = Server.MapPath("~/img/mail/" + fileName);
                file.SaveAs(path);

                // Tạo đường dẫn URL để nhúng vào email
                imageUrl = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content("~/img/mail/" + fileName);

                // Nhúng ảnh vào nội dung HTML
                body += $"<br><img src=\"{imageUrl}\" style=\"max-width:100%;\" />";
            }

            var emailHelper = new chuyende.Helper.SendMail();
            bool result = emailHelper.SendMailFunction(lienHe.Email, subject, body,file); // Không truyền file nữa

            TempData["Message"] = result ? "✅ Email đã gửi thành công!" : "❌ Gửi email thất bại.";
            return RedirectToAction("Index");
        }


    }
}
