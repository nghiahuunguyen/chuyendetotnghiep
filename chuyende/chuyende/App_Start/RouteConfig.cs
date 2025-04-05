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

            // Route chi tiết sản phẩm: /{loaiAlias}/{alias}
            routes.MapRoute(
                name: "ChiTietSanPham",
                url: "{loaiAlias}/{alias}",
                defaults: new { controller = "Module", action = "ChiTiet" },
                constraints: new { loaiAlias = @"^[a-zA-Z0-9\-]+$", alias = @"^[a-zA-Z0-9\-]+$" }
            );

            // Route loại sản phẩm: /{alias}
            routes.MapRoute(
                name: "AliasRoute",
                url: "{alias}",
                defaults: new { controller = "Module", action = "ByLoai" },
                constraints: new { alias = @"^[a-zA-Z0-9\-]+$" }
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
