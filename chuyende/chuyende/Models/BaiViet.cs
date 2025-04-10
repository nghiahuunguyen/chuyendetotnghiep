using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace chuyende.Models
{
    [Table("BaiViet")]
    public class BaiViet
	{
        [Key]
        public string MaBV { get; set; }
        [Required]
        public string TenBV { get; set; }
        public string NoiDung { get; set; }
        public string HinhAnh { get; set; }
        public string Link{ get; set; }
        public string MaLoaiBV { get; set; }
        public int Status { get; set; }
        public virtual LoaiBaiViet LoaiBaiViet { get; set; }
    }
}