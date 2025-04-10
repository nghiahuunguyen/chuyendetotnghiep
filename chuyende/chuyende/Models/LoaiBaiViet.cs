using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace chuyende.Models
{
    [Table("LoaiBaiViet")]
    public class LoaiBaiViet
    {
        [Key]
        public string MaLoaiBV { get; set; }
        [Required]
        public string TenLoaiBV { get; set; }

        public int Status { get; set; }
        public virtual ICollection<BaiViet> BaiViets { get; set; }
    }
}