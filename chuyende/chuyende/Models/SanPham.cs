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
        [Required]
        public string MaLoaiSP { get; set; }
        [Required]
        public string MaHang { get; set; }
        [Required(ErrorMessage = "Tên sản phẩm không được để trống"), StringLength(255)]
        public string TenSP { get; set; }
        public string HinhAnh { get; set; }
        public int SoLuong { get; set; }
        public string KhuyenMai { get; set; }
        public string TuKhoa { get; set; }
        public decimal GiaNhap { get; set; }
        public decimal GiaDau { get; set; }
        public int SoGiam { get; set; }
        public string Chip { get; set; }
        public string Pin { get; set; }
        public string CongSac { get; set; }
        public string HeDieuHanh { get; set; }
        public string DungLuong { get; set; }
        public string Ram { get; set; }
        public string ManHinh { get; set; }
        public string TheSim { get; set; }
        public string CamTruoc { get; set; }
        public string CamSau { get; set; }
        public string Mau { get; set; }
        public string KetNoi { get; set; }
        [Required]
        public int Status { get; set; }

        public virtual Hang Hang { get; set; }
        public virtual LoaiSanPham LoaiSanPham { get; set; }
    }
}