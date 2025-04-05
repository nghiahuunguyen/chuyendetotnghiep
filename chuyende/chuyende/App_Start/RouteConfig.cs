using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace chuyende
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Route cho chi tiết sản phẩm (ModuleController)
            routes.MapRoute(
    name: "ChiTietSanPham",
    url: "{loaiAlias}/{alias}",
    defaults: new { controller = "Module", action = "ChiTiet" }
);



            // Route alias theo loại sản phẩm
            routes.MapRoute(
                name: "AliasRoute",
                url: "{alias}",
                defaults: new { controller = "Module", action = "ByLoai" }
            );

            // Default route
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }


}
