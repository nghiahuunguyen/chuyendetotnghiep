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
            if (lienHe != null)
            {
                lienHe.TrangThai = lienHe.TrangThai == 1 ? 0 : 1;
                db.SaveChanges();
            }
            return RedirectToAction("Index");

        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return RedirectToAction("Index");
            LienHe lienHe = db.LienHes.Find(id);
            if (lienHe == null)
                return RedirectToAction("Index");
            return View(lienHe);
        }

    }
}
