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
            routes.MapRoute(
               name: "Cart",
               url: "Cart/{action}/{id}",
               defaults: new { controller = "Cart", action = "Index", id = UrlParameter.Optional }
           );
            routes.MapRoute(
                name: "User",
                url: "User/{action}/{id}",
                defaults: new { controller = "User", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
              name: "Register",
              url: "Register/{action}/{id}",
              defaults: new { controller = "Register", action = "Index", id = UrlParameter.Optional }
          );


            routes.MapRoute(
                name: "Login",
                url: "Login/{action}/{id}",
                defaults: new { controller = "Login", action = "Index", id = UrlParameter.Optional }
            );
            // Route lọc theo hãng – đặt TRƯỚC
            routes.MapRoute(
                name: "ByHang",
                url: "{loaiAlias}/hang/{hangAlias}",
                defaults: new { controller = "Module", action = "ByHang" },
                constraints: new { loaiAlias = @"^[a-zA-Z0-9\-]+$", hangAlias = @"^[a-zA-Z0-9\-]+$" }
            );

            // Route chi tiết sản phẩm – đặt SAU
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