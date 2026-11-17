using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _25DH110028_MyStore.Models;
using PagedList;

namespace _25DH110028_MyStore.Areas.Admin.Controllers
{
    public class OrdersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Orders (Xem danh sách tất cả đơn hàng)
        public ActionResult Index(string searchTerm, int? page)
        {
            var orders = db.Orders.Include(o => o.Customer).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                int orderId;
                bool isId = int.TryParse(searchTerm, out orderId);

                orders = orders.Where(o => (isId && o.OrderID == orderId) ||
                                           o.Customer.CustomerName.Contains(searchTerm) ||
                                           o.PaymentStatus.Contains(searchTerm));
                ViewBag.SearchTerm = searchTerm;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            return View(orders.OrderByDescending(o => o.OrderDate).ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/Orders/Details/5 (Xem chi tiết đơn hàng và các sản phẩm trong đơn)
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Order order = db.Orders
                            .Include(o => o.Customer)
                            .Include(o => o.OrderDetails.Select(d => d.Product))
                            .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // ==========================================
        // XÁC NHẬN / CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmOrder(int id, string paymentStatus)
        {
            var order = db.Orders.Find(id);
            if (order != null)
            {
                // Cập nhật trạng thái thanh toán / trạng thái đơn hàng
                order.PaymentStatus = paymentStatus;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật trạng thái đơn hàng thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng cần cập nhật.";
            }

            return RedirectToAction("Index");
        }

        // ==========================================
        // XÓA ĐƠN HÀNG
        // ==========================================
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var order = db.Orders
                          .Include(o => o.OrderDetails)
                          .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            try
            {
                // Xóa các chi tiết đơn hàng liên quan trước để tránh lỗi ràng buộc khóa ngoại (FK)
                if (order.OrderDetails != null && order.OrderDetails.Any())
                {
                    db.OrderDetails.RemoveRange(order.OrderDetails);
                }

                // Xóa đơn hàng chính
                db.Orders.Remove(order);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Đã xóa đơn hàng thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Không thể xóa đơn hàng này do vướng dữ liệu liên quan: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

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