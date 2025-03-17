using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace chuyende.Models
{
    [Table("NhanVien")]
    public class NhanVien
    {
        [Key]
        public string MaNV { get; set; }
        [Required(ErrorMessage = "Tên nhân viên không được để trống"), StringLength(255)]
        public string TenNV { get; set; }
        [Required(ErrorMessage = "Số điện thoại không được để trống"), StringLength(10)]
        public string SoDienThoai { get; set; }
        [Required(ErrorMessage = "Email không hợp lệ"), EmailAddress]
        public string Email { get; set; }
        public DateTime NgaySinh { get; set; }
        public bool GioiTinh { get; set; }
        [Required(ErrorMessage = "CCCD không được để trống"), StringLength(12)]
        public string CCCD { get; set; }
        [Required(ErrorMessage = "Địa chỉ không được để trống"), StringLength(500)]
        public string DiaChi { get; set; }
        [Required(ErrorMessage = "Tên đăng nhập không được để trống"), StringLength(255)]
        public string TenDN { get; set; }
        [Required]
        public string MatKhau { get; set; }
        [Required]
        public string MaCV { get; set; }
        public int Status { get; set; }
        public virtual ICollection<HoaDon> HoaDons { get; set; }
        public virtual ChucVu ChucVu { get; set; }
    }
}