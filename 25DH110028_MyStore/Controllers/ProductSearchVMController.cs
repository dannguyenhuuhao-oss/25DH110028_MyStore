using _25DH110028_MyStore.Models;
using _25DH110028_MyStore.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList; // Bổ sung thư viện PagedList

namespace _25DH110028_MyStore.Controllers
{
    public class ProductSearchVMController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // Bổ sung tham số int? page
        public ActionResult Index(string searchTerm, int? page)
        {
            var model = new ProductSearchVM();
            var products = db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Lưu lại từ khóa để view có thể truyền tiếp sang các trang khác
                model.SearchTerm = searchTerm;

                products = products.Where(p =>
                    p.ProductName.Contains(searchTerm) ||
                    p.ProductDescription.Contains(searchTerm) ||
                    p.Category.CategoryName.Contains(searchTerm));
            }

            // LINQ to Entities yêu cầu dữ liệu phải được sắp xếp trước khi dùng Skip/Take (ToPagedList)
            products = products.OrderBy(p => p.ProductName);

            // Thiết lập thông số phân trang
            int pageNumber = page ?? 1; // Mặc định là trang 1 nếu không có tham số page
            int pageSize = 10; // Số lượng sản phẩm trên mỗi trang (có thể tùy chỉnh)

            // Sử dụng ToPagedList thay vì ToList()
            model.Products = products.ToPagedList(pageNumber, pageSize);

            return View(model);
        }
    }
}