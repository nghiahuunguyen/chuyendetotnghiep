using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using chuyende.Models;

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
    }

}