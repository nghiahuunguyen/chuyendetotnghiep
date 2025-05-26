using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace chuyende.Models
{
    public class ThongKeTongHopViewModel
    {
        public List<ThongKeDoanhThu> DoanhThu { get; set; }
        public List<SanPhamBanChay> SanPhamBanChay { get; set; }
    }
}