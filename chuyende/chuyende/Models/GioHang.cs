using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace chuyende.Models
{
	public class GioHang
	{
        [Key]
        public string MaGioHang { get; set; }

        [Required]
        public string MaKH { get; set; }

        [ForeignKey("MaKH")]
        public virtual KhachHang KhachHang { get; set; }

        public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; }
    }
}