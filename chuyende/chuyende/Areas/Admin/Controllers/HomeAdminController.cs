using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace chuyende.Areas.Admin.Controllers
{
    public class HomeAdminController : Controller
    {
        // GET: Admin/HomeAdmin
        public ActionResult Index()
        {
            //if (Session["User"] == null)
            //{
            //    return RedirectToAction("Index", "DangNhap");
            //}
            return View();
        }
    }
}