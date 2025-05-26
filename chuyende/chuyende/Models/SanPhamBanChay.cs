using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace chuyende.Models
{
	public class SanPhamBanChay
	{
        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public string HinhAnh { get; set; }
        public int? Ngay { get; set; }
        public int? Thang { get; set; }
        public int Nam { get; set; }
        public int TongSoLuong { get; set; }
    }
}