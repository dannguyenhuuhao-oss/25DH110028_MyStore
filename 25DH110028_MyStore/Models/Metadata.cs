using System;
using System.ComponentModel.DataAnnotations;

namespace _25DH110028_MyStore.Models
{
    // =====================================================
    //  USER
    // =====================================================
    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {
    }

    public class UserMetadata
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập tối đa 50 ký tự")]
        public string Username { get; set; }

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Mật khẩu tối đa 100 ký tự")]
        public string Password { get; set; }

        [Display(Name = "Vai trò")]
        [Required(ErrorMessage = "Vui lòng nhập vai trò")]
        public string UserRole { get; set; }          // đúng tên cột; KHÔNG viết là Role
    }

    // =====================================================
    //  CUSTOMER
    // =====================================================
    [MetadataType(typeof(CustomerMetadata))]
    public partial class Customer
    {
    }

    public class CustomerMetadata
    {
        [Display(Name = "Mã khách hàng")]
        public int CustomerID { get; set; }

        [Display(Name = "Họ tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        public string CustomerName { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại phải gồm 10–11 số và bắt đầu bằng 0")]
        public string CustomerPhone { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string CustomerEmail { get; set; }

        [Display(Name = "Địa chỉ")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [StringLength(250, ErrorMessage = "Địa chỉ tối đa 250 ký tự")]
        public string CustomerAddress { get; set; }

        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }
    }

    // =====================================================
    //  CATEGORY
    // =====================================================
    [MetadataType(typeof(CategoryMetadata))]
    public partial class Category
    {
    }

    public class CategoryMetadata
    {
        [Display(Name = "Mã danh mục")]
        public int CategoryID { get; set; }

        [Display(Name = "Tên danh mục")]
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(100, ErrorMessage = "Tên danh mục tối đa 100 ký tự")]
        public string CategoryName { get; set; }
    }

    // =====================================================
    //  PRODUCT
    // =====================================================
    [MetadataType(typeof(ProductMetadata))]
    public partial class Product
    {
    }

    public class ProductMetadata
    {
        [Display(Name = "Mã sản phẩm")]
        public int ProductID { get; set; }

        [Display(Name = "Tên sản phẩm")]
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm tối đa 200 ký tự")]
        public string ProductName { get; set; }

        [Display(Name = "Mô tả")]
        [DataType(DataType.MultilineText)]
        public string ProductDescription { get; set; }

        [Display(Name = "Giá")]
        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Range(typeof(decimal), "0", "1000000000", ErrorMessage = "Giá phải từ 0 đến 1.000.000.000")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = false)]
        public decimal ProductPrice { get; set; }

        [Display(Name = "Hình ảnh")]
        public string ProductImage { get; set; }

        [Display(Name = "Danh mục")]
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryID { get; set; }

        [Display(Name = "Số lượng tồn")]
        [Range(0, 100000, ErrorMessage = "Số lượng tồn từ 0 đến 100.000")]
        public int Quantity { get; set; }

        [Display(Name = "Đã bán")]
        [Range(0, 100000, ErrorMessage = "Số lượng đã bán từ 0 đến 100.000")]
        public int SoldQuantity { get; set; }
    }
}