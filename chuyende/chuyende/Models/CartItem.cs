using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace chuyende.Models
{
	public class CartItem
	{
        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public string HinhAnh { get; set; }
        public decimal GiaBan { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien => GiaBan * SoLuong;
        public SanPham Product { get; set; } // Đối tượng SanPham đại diện cho sản phẩm
        public int Quantity { get; set; } // Số lượng của sản phẩm trong giỏ hàng
    }
}