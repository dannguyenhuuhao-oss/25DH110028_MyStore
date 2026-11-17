using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _25DH110028_MyStore.Models;
using PagedList; // Nếu có dùng phân trang, ngược lại có thể dùng List thông thường

namespace _25DH110028_MyStore.Areas.Admin.Controllers
{
    public class CustomersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Customers (Xem danh sách khách hàng kèm tìm kiếm)
        public ActionResult Index(string searchTerm, int? page)
        {
            var customers = db.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                customers = customers.Where(c => c.CustomerName.Contains(searchTerm) ||
                                                c.Username.Contains(searchTerm) ||
                                                c.CustomerPhone.Contains(searchTerm) ||
                                                c.CustomerEmail.Contains(searchTerm));
                ViewBag.SearchTerm = searchTerm;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            // Trả về danh sách có phân trang (hoặc dùng ToList() nếu không dùng PagedList)
            return View(customers.OrderBy(c => c.CustomerName).ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/Customers/Details/5 (Xem chi tiết khách hàng và các đơn hàng của họ)
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Lấy thông tin khách hàng kèm theo danh sách đơn hàng của họ
            Customer customer = db.Customers
                                  .Include(c => c.Orders)
                                  .FirstOrDefault(c => c.CustomerID == id);

            if (customer == null)
            {
                return HttpNotFound();
            }

            return View(customer);
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