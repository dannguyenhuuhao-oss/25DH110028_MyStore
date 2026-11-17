using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace _25DH110028_MyStore.Models
{
    // Phần mở rộng của User
    public partial class User
    {
        [NotMapped] // Không lưu vào CSDL
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }
    }

    // Phần mở rộng của Product
    public partial class Product
    {
        [NotMapped]
        public HttpPostedFileBase UploadImage { get; set; }

        [NotMapped]
        public List<Product> RemainProduct { get; set; }
    }
}