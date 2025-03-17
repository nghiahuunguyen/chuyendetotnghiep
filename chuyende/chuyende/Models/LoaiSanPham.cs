using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace chuyende.Models
{
    [Table("LoaiSanPham")]
    public class LoaiSanPham
    {
        [Key]
        public string MaLoaiSP { get; set; }
        [Required(ErrorMessage = "Tên loại sản phẩm không được để trống"), StringLength(255)]
        public string TenLoaiSP { get; set; }
        public int Status { get; set; }
        public virtual ICollection<SanPham> SanPhams { get; set; }
    }
}