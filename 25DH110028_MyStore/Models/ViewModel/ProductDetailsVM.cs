using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _25DH110028_MyStore.Models.ViewModel
{
    public class ProductDetailsVM
    {
        public Product product { get; set; }          // sản phẩm đang xem
        public int quantity { get; set; } = 1;        // số lượng định mua
        public decimal estimatedValue { get; set; }   // tạm tính = quantity * giá

        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 3;        // 3 sản phẩm / trang

        public List<Product> RelatedProducts { get; set; }     // 8 sản phẩm cùng danh mục
        public IPagedList<Product> TopProducts { get; set; }   // bán chạy cùng danh mục
    }
}
