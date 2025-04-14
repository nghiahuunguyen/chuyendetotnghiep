using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace chuyende.Models
{
    [Table("LienHe")]
    public class LienHe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaLH { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [StringLength(20)]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(200)]
        public string TieuDe { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string NoiDung { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime NgayGui { get; set; } = DateTime.Now;

        public int TrangThai { get; set; }
    }
}
