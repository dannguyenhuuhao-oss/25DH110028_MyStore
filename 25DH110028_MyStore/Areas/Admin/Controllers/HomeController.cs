using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _25DH110028_MyStore.Models;

namespace _25DH110028_MyStore.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // =====================================
        // BẢO VỆ KHU VỰC ADMIN: Kiểm tra quyền truy cập
        // =====================================
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            var userRole = Session["UserRole"] != null ? Session["UserRole"].ToString().Trim() : "";

            if (Session["Username"] == null || (!userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) && !userRole.Equals("A", StringComparison.OrdinalIgnoreCase)))
            {
                filterContext.Result = RedirectToAction("Login", "Account", new { area = "" });
            }
        }

        // GET: Admin/Home
        public ActionResult Index()
        {
            // =====================================
            // 1. THỐNG KÊ SỐ LƯỢNG
            // =====================================
            ViewBag.TotalProducts = db.Products.Count();
            ViewBag.TotalCategories = db.Categories.Count();
            ViewBag.TotalCustomers = db.Customers.Count();
            ViewBag.TotalUsers = db.Users.Count();
            ViewBag.TotalOrders = db.Orders.Count();

            // =====================================
            // 2. THỐNG KÊ TRẠNG THÁI THANH TOÁN
            // =====================================
            // Đơn chưa thanh toán
            ViewBag.UnpaidOrders = db.Orders.Count(
                o => o.PaymentStatus == "Chưa thanh toán" || string.IsNullOrEmpty(o.PaymentStatus)
            );

            // Đơn đã thanh toán (tất cả các đơn có trạng thái hoàn tất như: Thanh toán tiền mặt, Trả góp, Thanh toán paypal...)
            ViewBag.PaidOrders = db.Orders.Count(
                o => o.PaymentStatus != "Chưa thanh toán" && !string.IsNullOrEmpty(o.PaymentStatus)
            );

            // =====================================
            // 3. TỔNG DOANH THU
            // =====================================
            // Tính tổng tất cả các đơn hàng đã đặt thành công (trừ đơn chưa thanh toán)
            ViewBag.TotalRevenue = db.Orders
                .Where(o => o.PaymentStatus != "Chưa thanh toán" && !string.IsNullOrEmpty(o.PaymentStatus))
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            // =====================================
            // 4. DỮ LIỆU ĐỂ VẼ BIỂU ĐỒ DOANH THU THEO THÁNG
            // =====================================
            var revenueByMonth = db.Orders
                .Where(o => o.PaymentStatus != "Chưa thanh toán" && !string.IsNullOrEmpty(o.PaymentStatus) && o.OrderDate != null)
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new {
                    Month = g.Key,
                    Revenue = g.Sum(o => (decimal?)o.TotalAmount) ?? 0
                })
                .OrderBy(x => x.Month)
                .ToList();

            // Truyền dữ liệu dạng chuỗi để vẽ biểu đồ JavaScript
            ViewBag.Months = revenueByMonth.Any() ? string.Join(",", revenueByMonth.Select(x => "'Tháng " + x.Month + "'")) : "";
            ViewBag.Revenues = revenueByMonth.Any() ? string.Join(",", revenueByMonth.Select(x => x.Revenue)) : "";

            return View();
        }

        // Giải phóng kết nối database
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}