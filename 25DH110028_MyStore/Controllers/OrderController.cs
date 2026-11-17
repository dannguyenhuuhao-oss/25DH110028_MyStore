using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using _25DH110028_MyStore.Models;
using _25DH110028_MyStore.Models.ViewModel;

namespace _25DH110028_MyStore.Controllers
{
    [Authorize]     // toàn bộ controller yêu cầu đăng nhập
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        private CartService GetCartService()
        {
            return new CartService(Session);
        }

        // Khách hàng ứng với tài khoản đang đăng nhập
        private Customer GetCurrentCustomer()
        {
            return db.Customers.FirstOrDefault(c => c.Username == User.Identity.Name);
        }

        // ============ THANH TOÁN ============
        // GET: Order/Checkout
        public ActionResult Checkout()
        {
            // Giỏ hàng rỗng thì về trang chủ
            var cart = GetCartService().GetCart();
            if (!cart.Items.Any())
                return RedirectToAction("Index", "Home");

            // Chưa có thông tin khách hàng thì về trang đăng nhập
            var customer = GetCurrentCustomer();
            if (customer == null)
                return RedirectToAction("Login", "Account");

            var model = new CheckoutVM
            {
                CartItems = cart.Items.ToList(),
                TotalAmount = cart.TotalValue(),
                OrderDate = DateTime.Now,
                ShippingAddress = customer.CustomerAddress,    // địa chỉ mặc định
                CustomerID = customer.CustomerID,
                Username = customer.Username,
                ShippingMethod = "Giao hàng nhanh",            // lựa chọn mặc định
                PaymentMethod = "Tiền mặt"
            };
            return View(model);
        }

        // POST: Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutVM model)
        {
            var cart = GetCartService().GetCart();
            if (!cart.Items.Any())
                return RedirectToAction("Index", "Home");

            var customer = GetCurrentCustomer();
            if (customer == null)
                return RedirectToAction("Login", "Account");

            // Dữ liệu không hợp lệ: hiển thị lại form (nạp lại giỏ hàng để bảng không bị trống)
            if (!ModelState.IsValid)
            {
                model.CartItems = cart.Items.ToList();
                model.TotalAmount = cart.TotalValue();
                model.Username = customer.Username;
                return View(model);
            }

            // Kiểm tra số lượng tồn của từng sản phẩm
            var ids = cart.Items.Select(i => i.ProductID).ToList();
            var stockProducts = db.Products.Where(p => ids.Contains(p.ProductID)).ToList();
            foreach (var item in cart.Items)
            {
                var p = stockProducts.FirstOrDefault(x => x.ProductID == item.ProductID);
                if (p == null || p.Quantity < item.Quantity)
                    ModelState.AddModelError("", "Sản phẩm \"" + item.ProductName + "\" chỉ còn "
                                               + (p == null ? 0 : p.Quantity) + " trong kho.");
            }
            if (!ModelState.IsValid)
            {
                model.CartItems = cart.Items.ToList();
                model.TotalAmount = cart.TotalValue();
                model.Username = customer.Username;
                return View(model);
            }

            // --- TÍCH HỢP PAYPAL ---
            // Nếu khách hàng chọn phương thức thanh toán là Paypal, chuyển hướng sang PaypalController
            if (model.PaymentMethod == "Paypal")
            {
                model.CartItems = cart.Items.ToList();
                model.TotalAmount = cart.TotalValue();
                return RedirectToAction("PaymentWithPaypal", "Paypal", model);
            }

            // Trạng thái thanh toán ban đầu theo phương thức thanh toán thông thường
            string paymentStatus;
            switch (model.PaymentMethod)
            {
                case "Tiền mặt": paymentStatus = "Thanh toán tiền mặt"; break;
                case "Mua trước trả sau": paymentStatus = "Trả góp"; break;
                default: paymentStatus = "Chưa thanh toán"; break;
            }

            // Tạo đơn hàng và chi tiết đơn hàng
            var order = new _25DH110028_MyStore.Models.Order
            {
                CustomerID = customer.CustomerID,
                OrderDate = DateTime.Now,
                TotalAmount = cart.TotalValue(),          // tính lại từ giỏ, không tin dữ liệu từ form
                PaymentStatus = paymentStatus,
                PaymentMethod = model.PaymentMethod,
                DeliveryMethod = model.ShippingMethod,
                ShippingAddress = model.ShippingAddress,
                OrderDetails = cart.Items.Select(item => new _25DH110028_MyStore.Models.OrderDetail
                {
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList()
            };

            try
            {
                // Trừ tồn kho, cộng số lượng đã bán
                foreach (var item in cart.Items)
                {
                    var p = stockProducts.First(x => x.ProductID == item.ProductID);
                    p.Quantity -= item.Quantity;
                    p.SoldQuantity += item.Quantity;
                }

                // Thêm và lưu đơn hàng vào CSDL
                db.Orders.Add(order);
                db.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                foreach (var entry in ex.EntityValidationErrors)
                    foreach (var err in entry.ValidationErrors)
                        ModelState.AddModelError("", entry.Entry.Entity.GetType().Name + "." + err.PropertyName + ": " + err.ErrorMessage);

                model.CartItems = cart.Items.ToList();
                model.TotalAmount = cart.TotalValue();
                model.Username = customer.Username;
                return View(model);
            }
            catch (Exception ex)
            {
                var inner = ex;
                while (inner.InnerException != null) inner = inner.InnerException;
                ModelState.AddModelError("", "Không thể lưu đơn hàng: " + inner.Message);

                model.CartItems = cart.Items.ToList();
                model.TotalAmount = cart.TotalValue();
                model.Username = customer.Username;
                return View(model);
            }

            // --- ĐẶT HÀNG THÀNH CÔNG ---
            GetCartService().ClearCart();
            Session["Cart"] = null;

            TempData["SuccessMessage"] = "Chúc mừng bạn đã đặt hàng thành công! Đơn hàng của bạn đang được xử lý.";

            return RedirectToAction("OrderSuccess", new { id = order.OrderID });
        }

        // ============ XÁC NHẬN ĐƠN HÀNG ============
        public ActionResult OrderSuccess(int id)
        {
            var order = db.Orders.Include("OrderDetails.Product").Include("Customer")
                                 .SingleOrDefault(o => o.OrderID == id);
            if (order == null) return HttpNotFound();

            // Chỉ chủ đơn hàng mới được xem
            if (order.Customer == null || order.Customer.Username != User.Identity.Name)
                return HttpNotFound();

            return View(order);
        }

        // ============ ĐƠN HÀNG CỦA TÔI (HỖ TRỢ CÁC TAB TRẠNG THÁI) ============
        public ActionResult MyOrder(string tab, string searchTerm)
        {
            var customer = GetCurrentCustomer();
            if (customer == null) return HttpNotFound();

            IQueryable<Order> query = db.Orders.Include("OrderDetails.Product")
                                               .Where(o => o.CustomerID == customer.CustomerID);

            // Tìm theo mã đơn hàng hoặc tên sản phẩm
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string kw = searchTerm.Trim();
                int id;
                bool isId = int.TryParse(kw, out id);
                query = query.Where(o => (isId && o.OrderID == id)
                                        || o.OrderDetails.Any(d => d.Product.ProductName.Contains(kw)));
            }

            var all = query.OrderByDescending(o => o.OrderDate).ToList();

            // Định nghĩa điều kiện lọc cho từng tab trạng thái
            Func<Order, bool> isPending = o => (o.PaymentStatus ?? "").Trim() == "Chưa thanh toán";
            Func<Order, bool> isProcessing = o => (o.PaymentStatus ?? "").Trim() == "Đã xác nhận"
                                               || (o.PaymentStatus ?? "").Trim() == "Thanh toán tiền mặt"
                                               || (o.PaymentStatus ?? "").Trim() == "Thanh toán paypal"
                                               || (o.PaymentStatus ?? "").Trim() == "Trả góp";
            Func<Order, bool> isShipping = o => (o.PaymentStatus ?? "").Trim() == "Đang giao hàng";
            Func<Order, bool> isDelivered = o => (o.PaymentStatus ?? "").Trim() == "Đã hoàn thành"
                                              || (o.PaymentStatus ?? "").Trim() == "Đã giao";
            Func<Order, bool> isCancelled = o => (o.PaymentStatus ?? "").Trim() == "Đã hủy";

            ViewBag.Tab = string.IsNullOrEmpty(tab) ? "all" : tab;
            ViewBag.SearchTerm = searchTerm;

            // Đếm số lượng cho từng tab hiển thị trên giao diện
            ViewBag.CountAll = all.Count;
            ViewBag.CountPending = all.Count(isPending);
            ViewBag.CountProcessing = all.Count(isProcessing);
            ViewBag.CountShipping = all.Count(isShipping);
            ViewBag.CountDelivered = all.Count(isDelivered);
            ViewBag.CountCancelled = all.Count(isCancelled);

            // Lọc dữ liệu theo tab đang chọn
            IEnumerable<Order> shown = all;
            if (tab == "pending") shown = all.Where(isPending);
            else if (tab == "processing") shown = all.Where(isProcessing);
            else if (tab == "shipping") shown = all.Where(isShipping);
            else if (tab == "delivered") shown = all.Where(isDelivered);
            else if (tab == "cancelled") shown = all.Where(isCancelled);

            return View(shown.ToList());
        }

        // Mua lại: đưa các sản phẩm của đơn hàng cũ vào giỏ
        public ActionResult Reorder(int id)
        {
            var customer = GetCurrentCustomer();
            if (customer == null) return HttpNotFound();

            var order = db.Orders.Include("OrderDetails.Product.Category")
                                 .FirstOrDefault(o => o.OrderID == id && o.CustomerID == customer.CustomerID);
            if (order == null) return HttpNotFound();

            var cart = GetCartService().GetCart();
            foreach (var d in order.OrderDetails)
            {
                cart.AddItem(d.ProductID, d.Product.ProductImage, d.Product.ProductName,
                             d.Product.ProductPrice, d.Quantity, d.Product.Category.CategoryName);
            }
            return RedirectToAction("Index", "Cart");
        }
    }
}