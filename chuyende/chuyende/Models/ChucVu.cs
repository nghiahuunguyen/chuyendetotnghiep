using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace chuyende.Models
{
    [Table("ChucVu")]
    public class ChucVu
    {
        [Key]
        public string MaCV { get; set; }
        [Required(ErrorMessage = "Tên chức vụ không được để trống"), StringLength(255)]
        public string TenCV { get; set; }
        public int Status { get; set; }
    }
}