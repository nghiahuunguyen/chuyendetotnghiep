using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace chuyende.Models
{
    [Table("SanPham")]
    public class SanPham
    {
        [Key]
        public string MaSP { get; set; }
        [Required(ErrorMessage = "Loại sản phẩm không được để trống")]
        public string MaLoaiSP { get; set; }
        [Required(ErrorMessage = "Tên hãng không được để trống")]
        public string MaHang { get; set; }
        [Required(ErrorMessage = "Tên sản phẩm không được để trống"), StringLength(255)]
        public string TenSP { get; set; }
        [Required(ErrorMessage = "Hình không được để trống")]
        public string HinhAnh { get; set; }
        [Required(ErrorMessage = "Số lượng không được để trống")]
        public int SoLuong { get; set; }
        [Required(ErrorMessage = "Khuyến mãi không được để trống")]
        public string KhuyenMai { get; set; }
        [Required(ErrorMessage = "Từ khóa không được để trống")]
        public string TuKhoa { get; set; }
        [Required(ErrorMessage = "Giá nhập không được để trống")]
        public decimal? GiaNhap { get; set; }
        [Required(ErrorMessage = "Giá ban đầu không được để trống")]
        public decimal? GiaDau { get; set; }
        [Required(ErrorMessage = "Số giảm % không được để trống")]
        public int? SoGiam { get; set; }
        [Required(ErrorMessage = "Mô tả không được để trống")]
        public string MoTa { get; set; }
        public int Status { get; set; }
        public int BanChay { get; set; }
        [Required(ErrorMessage = "Link không được để trống")]
        public string Link { get; set; }

        public virtual Hang Hang { get; set; }
        public virtual LoaiSanPham LoaiSanPham { get; set; }
        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDon { get; set; }
        public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; }
    }
}