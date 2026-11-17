using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace _25DH110028_MyStore.Models.ViewModel
{
    /* Lưu thông tin form Checkout */
    public class CheckoutVM
    {
        public List<CartItem> CartItems { get; set; }

        public int CustomerID { get; set; }
        public string Username { get; set; }

        [Display(Name = "Ngày đặt hàng")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Tổng giá trị")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Trạng thái thanh toán")]
        public string PaymentStatus { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức giao hàng")]
        [Display(Name = "Phương thức giao hàng")]
        public string ShippingMethod { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(250, ErrorMessage = "Địa chỉ tối đa 250 ký tự")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }
    }
}