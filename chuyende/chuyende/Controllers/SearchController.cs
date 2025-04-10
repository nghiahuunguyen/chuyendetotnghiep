using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Tokenizer.Symbols;

namespace chuyende.Controllers
{
    public class SearchController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        public ActionResult Index(string query)
        {
            var ketQua = db.SanPhams
                .Where(sp => (sp.TenSP.Contains(query) || sp.TuKhoa.Contains(query)) && sp.Status == 1)
                .ToList();

            ViewBag.TuKhoa = query;
            return View(ketQua); // Trả List<SanPham>
        }

        [HttpGet]
        public JsonResult Suggest(string term)
        {
            var suggestions = db.SanPhams
                .Where(sp => sp.TenSP.Contains(term) && sp.Status == 1)
                .Select(sp => new { label = sp.TenSP, value = sp.TenSP })
                .Take(10)
                .ToList();

            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }

    }

}