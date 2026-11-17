using _25DH110028_MyStore.Models;
using _25DH110028_MyStore.Models.ViewModel;
using PagedList;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace _25DH110028_MyStore.Controllers
{
    public class CartController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // Lấy dịch vụ giỏ hàng (gắn với Session hiện tại)
        private CartService GetCartService()
        {
            return new CartService(Session);
        }

        // Hiển thị giỏ hàng
        // Hiển thị giỏ hàng đã gom nhóm + sản phẩm tương tự
        public ActionResult Index(int? page)
        {
            var cart = GetCartService().GetCart();

            // Danh mục và mã các sản phẩm đang có trong giỏ
            var categories = cart.Items.Select(i => i.Category).Distinct().ToList();
            var inCartIds = cart.Items.Select(i => i.ProductID).ToList();

            // Cùng danh mục nhưng chưa có trong giỏ
            var similar = db.Products
                .Where(p => categories.Contains(p.Category.CategoryName)
                         && !inCartIds.Contains(p.ProductID))
                .OrderBy(p => p.ProductID);

            int pageNumber = page ?? 1;
            cart.SimilarProducts = similar.ToPagedList(pageNumber, cart.PageSize);
            return View(cart);
        }
        // Thêm sản phẩm vào giỏ
        [HttpGet] // Hoặc giữ nguyên nếu gọi bằng thẻ a href
        public ActionResult AddToCart(int id, int quantity = 1)
        {
            if (quantity < 1) quantity = 1;

            var product = db.Products.Find(id);
            if (product != null && product.Quantity > 0)
            {
                var cart = GetCartService().GetCart();
                var existing = cart.Items.FirstOrDefault(i => i.ProductID == id);
                int already = existing == null ? 0 : existing.Quantity;
                int allowed = product.Quantity - already;      // còn được thêm bao nhiêu

                if (allowed <= 0)
                {
                    TempData["CartMessage"] = "\"" + product.ProductName + "\" đã đạt số lượng tồn tối đa.";
                }
                else
                {
                    int q = Math.Min(quantity, allowed);
                    cart.AddItem(product.ProductID, product.ProductImage, product.ProductName,
                                 product.ProductPrice, q, product.Category.CategoryName);
                    if (q < quantity)
                        TempData["CartMessage"] = "Chỉ còn " + product.Quantity + " sản phẩm \"" + product.ProductName + "\", đã thêm tối đa.";
                }
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            var product = db.Products.Find(id);
            if (product != null && quantity >= 1)
            {
                if (quantity > product.Quantity)
                {
                    quantity = product.Quantity;
                    TempData["CartMessage"] = "Chỉ còn " + product.Quantity + " sản phẩm \"" + product.ProductName + "\".";
                }
                if (quantity >= 1)
                    GetCartService().GetCart().UpdateQuantity(id, quantity);
            }
            return RedirectToAction("Index");
        }
        // Xóa sản phẩm khỏi giỏ
        public ActionResult RemoveFromCart(int id)
        {
            GetCartService().GetCart().RemoveItem(id);
            return RedirectToAction("Index");
        }

        // Làm trống giỏ hàng
        public ActionResult ClearCart()
        {
            GetCartService().ClearCart();
            return RedirectToAction("Index");
        }
    }
}