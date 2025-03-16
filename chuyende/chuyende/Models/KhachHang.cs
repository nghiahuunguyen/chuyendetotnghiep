using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace chuyende.Models
{
    [Table("KhachHang")]
    public class KhachHang
    {
        [Key]
        public string MaKH { get; set; }
        [Required(ErrorMessage = "Tên khách hàng không được để trống"), StringLength(255)]
        public string TenKH { get; set; }
        public DateTime NgaySinh { get; set; }
        [Required(ErrorMessage = "Số điện thoại không được để trống"), StringLength(20)]
        public string SoDienThoai { get; set; }
        [Required(ErrorMessage = "Email không hợp lệ"), EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Địa chỉ không được để trống"), StringLength(500)]
        public string DiaChi { get; set; }
        [Required]
        public string MatKhau { get; set; }
        public virtual ICollection<HoaDon> HoaDons { get; set; }
    }
}