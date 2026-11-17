using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace _25DH110028_MyStore.Models.ViewModel
{
    public class Cart
    {
        private List<CartItem> items = new List<CartItem>();

        public IEnumerable<CartItem> Items => items;

        // Thêm sản phẩm: đã có trong giỏ thì cộng dồn số lượng
        public void AddItem(int productId, string productImage, string productName,
                            decimal unitPrice, int quantity, string category)
        {
            var existingItem = items.FirstOrDefault(i => i.ProductID == productId);
            if (existingItem == null)
            {
                items.Add(new CartItem
                {
                    ProductID = productId,
                    ProductImage = productImage,
                    ProductName = productName,
                    UnitPrice = unitPrice,
                    Quantity = quantity,
                    Category = category
                });
            }
            else
            {
                existingItem.Quantity += quantity;
            }
        }

        // Xóa một sản phẩm khỏi giỏ
        public void RemoveItem(int productId)
        {
            items.RemoveAll(i => i.ProductID == productId);
        }

        // Tổng giá trị giỏ hàng
        public decimal TotalValue()
        {
            return items.Sum(i => i.TotalPrice);
        }

        // Tổng số lượng (dùng cho số đếm cạnh biểu tượng giỏ hàng ở menu sau này)
        public int TotalQuantity()
        {
            return items.Sum(i => i.Quantity);
        }

        // Làm trống giỏ
        public void Clear()
        {
            items.Clear();
        }

        // Cập nhật số lượng
        public void UpdateQuantity(int productId, int quantity)
        {
            var item = items.FirstOrDefault(i => i.ProductID == productId);
            if (item != null)
            {
                item.Quantity = quantity;
            }
        }
        // Nhóm các sản phẩm trong giỏ theo tên danh mục
        public List<IGrouping<string, CartItem>> GroupedItems
            => items.GroupBy(i => i.Category).ToList();

        // Hỗ trợ phân trang cho sản phẩm tương tự
        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 6;     // 6 sản phẩm / trang

        // Sản phẩm cùng danh mục với các sản phẩm trong giỏ
        public IPagedList<Product> SimilarProducts { get; set; }
    }
}