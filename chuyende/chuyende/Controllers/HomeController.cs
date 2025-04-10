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

        // GET: Article/Footer
        public ActionResult Footer()
        {
            var loaiBaiViets = db.LoaiBaiViets
                .Include(l => l.BaiViets)
                .Where(l => l.BaiViets.Any(bv => bv.Status == 1))
                .ToList();

            return PartialView("_FooterArticles", loaiBaiViets);
        }
        public ActionResult Article(string link)
        {
            if (string.IsNullOrEmpty(link))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var baiViet = db.BaiViets.FirstOrDefault(b => b.Link == link && b.Status == 1);
            if (baiViet == null)
            {
                return HttpNotFound();
            }

            return View(baiViet);
        }

    }

}