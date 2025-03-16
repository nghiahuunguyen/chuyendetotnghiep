using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace chuyende.Models
{
    [Table("ChiTietHoaDon")]
    public class ChiTietHoaDon
    {
        [Key]
        public string ID { get; set; }
        public string MaHD { get; set; }
        public string MaSP { get; set; }
        public int SoLuong { get; set; }
    }
}