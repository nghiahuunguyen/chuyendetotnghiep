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
    public class LienHesController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        // GET: Admin/LienHes
        public ActionResult Index()
        {
            return View(db.LienHes.ToList());
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

    }
}
