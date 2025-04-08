using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace chuyende.Models
{
	public class ChiTietGioHang
	{
        [Key]
        public string MaChiTiet { get; set; }

        [Required]
        public string MaGioHang { get; set; }

        [ForeignKey("MaGioHang")]
        public virtual GioHang GioHang { get; set; }
        [Required]
        public int SoLuong { get; set; }
        [Required]
        [StringLength(10)]
        public string MaSP { get; set; }

        [ForeignKey("MaSP")]
        public virtual SanPham SanPham { get; set; }
    }
}