using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace chuyende.Models
{
    [Table("HoaDon")]
    public class HoaDon
    {
        [Key]
        public string MaHD { get; set; }
        public string TenKH { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public int PhuongThucThanhToan { get; set; }
        public int TrangThai { get; set; }
        public string NguoiTao { get; set; }
        public DateTime NgayTao { get; set; }
        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDon { get; set; }
    }
}