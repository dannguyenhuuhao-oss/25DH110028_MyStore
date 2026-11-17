using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _25DH110028_MyStore.Models.ViewModel
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public string Category { get; set; }        // tên danh mục (dùng cho 2.2)
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Thành tiền của dòng hàng này
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}