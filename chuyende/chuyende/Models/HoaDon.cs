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
        [Required]
        public string MaKH { get; set; }
        public int PhuongThucThanhToan { get; set; }
        public int TrangThai { get; set; }
        [Required]
        public string NguoiTao { get; set; }
        public DateTime NgayTao { get; set; }
        public virtual KhachHang KhachHang { get; set; }
        public virtual NhanVien NhanVien { get; set; }
    }
}