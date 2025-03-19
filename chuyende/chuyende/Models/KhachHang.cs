using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace chuyende.Models
{
    public class KhachHang
    {
        [Key]
        [StringLength(10)]
        public string MaKH { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống!")]
        [StringLength(100)]
        public string TenKH { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống!")]
        [StringLength(15)]
        [Phone]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Email không được để trống!")]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống!")]
        [StringLength(255)]
        public string MatKhau { get; set; }
    }
}
