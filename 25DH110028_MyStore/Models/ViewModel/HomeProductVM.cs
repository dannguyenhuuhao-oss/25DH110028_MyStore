using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace _25DH110028_MyStore.Models.ViewModel
{
        public class HomeProductVM
        {
            // từ khóa tìm kiếm (theo tên, mô tả hoặc loại sản phẩm)
            public string SearchTerm { get; set; }

            // hỗ trợ phân trang
            public int PageNumber { get; set; }
            public int PageSize { get; set; } = 10;

            // top 10 sản phẩm bán chạy (dạng slider)
            public List<Product> FeaturedProducts { get; set; }

            // 20 sản phẩm, đã phân trang
            public IPagedList<Product> NewProducts { get; set; }
        }
}