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
        [Required(ErrorMessage = "Tên hãng không được để trống")]
        public string TenHang { get; set; }
        [Required(ErrorMessage = "Logo không được để trống")]
        public string Logo { get; set; }
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        public string SoDienThoai { get; set; }
        [Required(ErrorMessage = "Email không được để trống")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        public string DiaChi { get; set; }
        [Required(ErrorMessage = "Từ khóa không được để trống")]
        public string TuKhoa { get; set; }
        public int Status { get; set; }
        [Required(ErrorMessage = "Link không được để trống")]
        public string Link { get; set; }
        public virtual ICollection<SanPham> SanPhams { get; set; }
    }
}