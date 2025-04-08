using chuyende.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace chuyende.Controllers
{
    public class CartController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        // Hàm lấy mã giỏ hàng từ người dùng đăng nhập
        private string GetCartId()
        {
            var user = Session["User"] as KhachHang;
            if (user == null) return null;

            var gioHang = db.GioHangs.FirstOrDefault(g => g.MaKH == user.MaKH);
            if (gioHang != null) return gioHang.MaGioHang;

            // Nếu chưa có giỏ hàng, tạo mới
            gioHang = new GioHang
            {
                MaGioHang = Guid.NewGuid().ToString(),
                MaKH = user.MaKH
            };
            db.GioHangs.Add(gioHang);
            db.SaveChanges();
            return gioHang.MaGioHang;
        }

        public ActionResult Index()
        {
            if (Session["User"] == null)
            {
                TempData["Message"] = "Vui lòng đăng nhập để xem giỏ hàng.";
                return RedirectToAction("Index", "Login");
            }

            string maKH = (Session["User"] as KhachHang).MaKH;
            var gioHang = db.GioHangs.FirstOrDefault(g => g.MaKH == maKH);

            if (gioHang == null)
                return View(new List<ChiTietGioHang>());

            var chiTiets = db.ChiTietGioHangs
                .Where(c => c.MaGioHang == gioHang.MaGioHang)
                .ToList();

            foreach (var item in chiTiets)
                item.SanPham = db.SanPhams.Find(item.MaSP);

            ViewBag.TongTien = chiTiets.Sum(c =>
            {
                var sp = c.SanPham;
                decimal gia = (sp.GiaDau ?? 0) * (1 - (decimal)(sp.SoGiam ?? 0) / 100);
                return gia * c.SoLuong;
            });

            return View(chiTiets);
        }

        public ActionResult AddToCart(string id)
        {
            if (Session["User"] == null)
            {
                TempData["Message"] = "Bạn cần đăng nhập để thêm sản phẩm.";
                return RedirectToAction("Index", "Login");
            }

            var product = db.SanPhams.Find(id);
            if (product == null) return HttpNotFound();

            string cartId = GetCartId();
            var chiTiet = db.ChiTietGioHangs.FirstOrDefault(c => c.MaGioHang == cartId && c.MaSP == id);

            if (chiTiet == null)
            {
                chiTiet = new ChiTietGioHang
                {
                    MaChiTiet = Guid.NewGuid().ToString(),
                    MaGioHang = cartId,
                    MaSP = id,
                    SoLuong = 1
                };
                db.ChiTietGioHangs.Add(chiTiet);
            }
            else
            {
                chiTiet.SoLuong++;
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UpdateQuantity(string id, string action)
        {
            string cartId = GetCartId();
            var item = db.ChiTietGioHangs.FirstOrDefault(x => x.MaSP == id && x.MaGioHang == cartId);

            if (item != null)
            {
                if (action == "increase")
                {
                    item.SoLuong++;
                }
                else if (action == "decrease")
                {
                    item.SoLuong--;
                }

                if (item.SoLuong <= 0)
                {
                    db.ChiTietGioHangs.Remove(item);
                }

                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }



        public ActionResult RemoveFromCart(string id)
        {
            string cartId = GetCartId();
            var chiTiet = db.ChiTietGioHangs.FirstOrDefault(c => c.MaSP == id && c.MaGioHang == cartId);

            if (chiTiet != null)
            {
                db.ChiTietGioHangs.Remove(chiTiet);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult ClearCart()
        {
            string cartId = GetCartId();
            var chiTiets = db.ChiTietGioHangs.Where(c => c.MaGioHang == cartId);
            db.ChiTietGioHangs.RemoveRange(chiTiets);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
        public PartialViewResult CartCount()
        {
            int cartCount = 0;
            if (Session["User"] != null)
            {
                var user = (KhachHang)Session["User"];
                var db = new QuanLyBanDienTuContext();

                cartCount = db.ChiTietGioHangs
                              .Where(x => x.MaGioHang == user.MaKH)
                              .Sum(x => (int?)x.SoLuong) ?? 0;
            }

            ViewBag.CartCount = cartCount;
            return PartialView("_CartCount");
        }

    }
}
