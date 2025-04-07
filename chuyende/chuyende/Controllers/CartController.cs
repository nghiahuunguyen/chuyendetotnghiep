using chuyende.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace chuyende.Controllers
{
    public class CartController : Controller
    {
        private QuanLyBanDienTuContext db = new QuanLyBanDienTuContext();

        private List<CartItem> GetCart()
        {
            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null)
            {
                cart = new List<CartItem>();
                Session["Cart"] = cart;
            }
            return cart;
        }

        public ActionResult Index()
        {
            var cart = GetCart();
            ViewBag.TongTien = cart.Sum(x => x.ThanhTien);
            return View(cart);
        }

        public ActionResult AddToCart(string id)
        {
            if (Session["User"] == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để thêm vào giỏ hàng.";
                return RedirectToAction("Index", "Login");
            }
            var product = db.SanPhams.Find(id);
            if (product == null)
                return HttpNotFound();

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MaSP == id);
            if (item == null)
            {
                decimal giaGoc = product.GiaDau ?? 0;
                int soGiam = product.SoGiam ?? 0; // Nếu SoGiam có thể null
                decimal donGia = giaGoc - (giaGoc * soGiam / 100);

                cart.Add(new CartItem
                {
                    MaSP = product.MaSP,
                    TenSP = product.TenSP,
                    HinhAnh = product.HinhAnh,
                    GiaBan = donGia,
                    Product =product,
                    SoLuong = 1
                });
            }
            else
            {
                item.SoLuong++;
            }

            return RedirectToAction("Index");
        }

        public ActionResult RemoveFromCart(string id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MaSP == id);
            if (item != null)
            {
                cart.Remove(item);
            }
            return RedirectToAction("Index");
        }

        public ActionResult UpdateQuantity(string id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MaSP == id);
            if (item != null)
            {
                item.SoLuong = quantity;
            }
            return RedirectToAction("Index");
        }

        public ActionResult ClearCart()
        {
            Session["Cart"] = null;
            return RedirectToAction("Index");
        }
    }
}
