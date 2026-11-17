using _25DH110028_MyStore.Models;
using _25DH110028_MyStore.Models.ViewModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace _25DH110028_MyStore.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        public ActionResult Index(string searchTerm, int? page)
        {
            var model = new HomeProductVM();

            // 1. Lấy tất cả sản phẩm (Có thể bỏ điều kiện Quantity > 0 nếu muốn hiển thị cả sản phẩm hết hàng, 
            // hoặc giữ lại nhưng đảm bảo khi thêm mới ở Admin bạn phải nhập số lượng > 0).
            // Ở đây ta sửa lại an toàn hơn: chỉ lấy sản phẩm có Quantity >= 0
            var products = db.Products.Where(p => p.Quantity >= 0).AsQueryable();

            // 2. Tìm kiếm sản phẩm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p => p.ProductName.Contains(searchTerm)
                                           || p.ProductDescription.Contains(searchTerm)
                                           || p.Category.CategoryName.Contains(searchTerm));
            }

            // 3. Thông số phân trang cho sản phẩm mới
            int pageNumber = page ?? 1;
            int pageSize = 6;                // 6 sản phẩm / trang

            // 4. Top 10 sản phẩm bán chạy nhất (Ưu tiên dùng SoldQuantity nếu có, nếu không dùng số lượng OrderDetails)
            model.FeaturedProducts = products
                .OrderByDescending(p => p.SoldQuantity) // Hoặc p.OrderDetails.Count()
                .Take(10).ToList();

            // 5. Danh sách sản phẩm mới (Sắp xếp giảm dần theo ProductID để sách vừa thêm ở Admin hiện lên đầu tiên)
            model.NewProducts = products
                .OrderByDescending(p => p.ProductID)
                .ToPagedList(pageNumber, pageSize);

            // 6. Thông tin khách hàng đang đăng nhập (hiển thị khi bấm vào tên trên thanh đầu)
            if (User.Identity.IsAuthenticated)
            {
                var cus = db.Customers.FirstOrDefault(c => c.Username == User.Identity.Name);
                if (cus != null)
                {
                    ViewBag.Customer = new ProfileVM
                    {
                        Username = cus.Username,
                        CustomerName = cus.CustomerName,
                        CustomerPhone = cus.CustomerPhone,
                        CustomerEmail = cus.CustomerEmail,
                        CustomerAddress = cus.CustomerAddress
                    };
                }
            }

            return View(model);
        }

        // Danh mục hiển thị trên thanh menu
        [ChildActionOnly]
        public ActionResult CategoriesPV()
        {
            var categories = db.Categories.OrderBy(c => c.CategoryName).ToList();
            return PartialView("_CategoriesPV", categories);
        }

        // Các ô tròn danh mục ở trang chủ
        [ChildActionOnly]
        public ActionResult CategoryTilesPV()
        {
            var categories = db.Categories.OrderBy(c => c.CategoryName).ToList();
            return PartialView("_CategoryTilesPV", categories);
        }

        public ActionResult ProductList(int? id, int? page)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            ViewBag.CategoryID = id;
            ViewBag.CategoryName = db.Categories.Find(id)?.CategoryName;

            var list = db.Products.Where(p => p.CategoryID == id && p.Quantity >= 0)
                                    .OrderBy(p => p.ProductName)
                                    .ToPagedList(page ?? 1, 12);
            return View(list);
        }

        // GET: Home/ProductDetails/5
        public ActionResult ProductDetails(int? id, int? quantity, int? page)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product pro = db.Products.Find(id);
            if (pro == null)
                return HttpNotFound();

            // các sản phẩm cùng danh mục, trừ chính sản phẩm đang xem
            var products = db.Products
                .Where(p => p.CategoryID == pro.CategoryID && p.ProductID != pro.ProductID && p.Quantity >= 0)
                .AsQueryable();

            var model = new ProductDetailsVM();
            int pageNumber = page ?? 1;

            model.product = pro;
            model.RelatedProducts = products.OrderBy(p => p.ProductID).Take(8).ToList();
            model.TopProducts = products.OrderByDescending(p => p.OrderDetails.Count())
                                        .Take(8)
                                        .ToPagedList(pageNumber, model.PageSize);

            if (quantity.HasValue && quantity.Value > 0)
                model.quantity = quantity.Value;

            model.estimatedValue = model.quantity * pro.ProductPrice;
            return View(model);
        }
    }
}