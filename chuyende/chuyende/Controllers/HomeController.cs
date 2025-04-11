using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using chuyende.Models;
using System.Data.Entity; 

namespace chuyende.Controllers
{
    public class HomeController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        public ActionResult Index()
        {
            var sanPhams = db.SanPhams
                             .Where(sp => sp.Status == 1 && sp.BanChay == 1)
                             .ToList();

            return View(sanPhams);
        }

        public ActionResult Info()
        {
            return View();
        }

        public ActionResult Recruitment()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(LienHe lienHe)
        {
            if (ModelState.IsValid)
            {
                lienHe.NgayGui = DateTime.Now;
                lienHe.TrangThai = 0;

                db.LienHes.Add(lienHe);
                db.SaveChanges();

                TempData["Success"] = "Cảm ơn bạn đã liên hệ. Chúng tôi sẽ phản hồi sớm nhất!";
                return RedirectToAction("Contact");
            }

            return View(lienHe);
        }

    }

}