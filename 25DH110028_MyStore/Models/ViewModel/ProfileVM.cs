using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace _25DH110028_MyStore.Models.ViewModel
{
    public class ProfileVM
    {
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }          // chỉ hiển thị, không cho sửa

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ tên")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại phải gồm 10–11 số và bắt đầu bằng 0")]
        public string CustomerPhone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string CustomerEmail { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [Display(Name = "Địa chỉ")]
        public string CustomerAddress { get; set; }
    }
}