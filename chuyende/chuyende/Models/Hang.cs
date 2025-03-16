using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace chuyende.Models
{
    [Table("Hang")]
    public class Hang
    {
        [Key]
        public string MaHang { get; set; }
        public string TenHang { get; set; }
        public string Logo { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public string TuKhoa { get; set; }
        public virtual ICollection<SanPham> SanPhams { get; set; }
    }
}