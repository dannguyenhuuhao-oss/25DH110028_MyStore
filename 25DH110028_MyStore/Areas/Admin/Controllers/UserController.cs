using _25DH110028_MyStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace _25DH110028_MyStore.Areas.Admin.Controllers
{
    public class UsersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Users
        public ActionResult Index(
            string searchString,
            string sortOrder,
            int? page,
            int? pageSize)
        {
            ViewBag.CurrentSort = sortOrder;

            ViewBag.UsernameSortParm =
                sortOrder == "username_asc" ? "username_desc" : "username_asc";

            ViewBag.RoleSortParm =
                sortOrder == "role_asc" ? "role_desc" : "role_asc";

            ViewBag.CurrentFilter = searchString;

            var users = db.Users.AsQueryable();

            // Tìm kiếm theo tên đăng nhập hoặc vai trò
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                users = users.Where(u =>
                    u.Username.Contains(searchString) ||
                    u.UserRole.Contains(searchString));
            }

            // Sắp xếp
            switch (sortOrder)
            {
                case "username_asc":
                    users = users.OrderBy(u => u.Username);
                    break;

                case "username_desc":
                    users = users.OrderByDescending(u => u.Username);
                    break;

                case "role_asc":
                    users = users.OrderBy(u => u.UserRole);
                    break;

                case "role_desc":
                    users = users.OrderByDescending(u => u.UserRole);
                    break;

                default:
                    users = users.OrderBy(u => u.Username);
                    break;
            }

            // Chọn số lượng tài khoản mỗi trang
            int selectedPageSize = pageSize ?? 5;

            if (selectedPageSize != 5 &&
                selectedPageSize != 10 &&
                selectedPageSize != 20)
            {
                selectedPageSize = 5;
            }

            // Tính số trang
            int pageNumber = page ?? 1;

            if (pageNumber < 1)
                pageNumber = 1;

            int totalItems = users.Count();

            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling((double)totalItems / selectedPageSize)
            );

            if (pageNumber > totalPages)
                pageNumber = totalPages;

            // Gửi thông tin sang View
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = selectedPageSize;

            var result = users
                .Skip((pageNumber - 1) * selectedPageSize)
                .Take(selectedPageSize)
                .ToList();

            return View(result);
        }

        // GET: Admin/Users/Details/duy123
        public ActionResult Details(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User user = db.Users.Find(username);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        // GET: Admin/Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "Username,Password,ConfirmPassword,UserRole")] User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    user.Username = user.Username.Trim();
                    user.UserRole = user.UserRole.Trim();
                    if (user == null ||
                        string.IsNullOrWhiteSpace(user.Username) ||
                        string.IsNullOrWhiteSpace(user.Password) ||
                        string.IsNullOrWhiteSpace(user.UserRole))
                    {
                        ModelState.AddModelError(
                            "",
                            "Vui lòng nhập đầy đủ thông tin tài khoản."
                        );

                        return View(user);
                    }

                    user.Username = user.Username.Trim();
                    user.UserRole = user.UserRole.Trim();

                    // Kiểm tra tên đăng nhập đã tồn tại
                    var existingUser = db.Users.Find(user.Username);

                    if (existingUser != null)
                    {
                        ModelState.AddModelError(
                            "",
                            "Tên đăng nhập đã tồn tại!"
                        );

                        return View(user);
                    }

                    db.Users.Add(user);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Thêm tài khoản thành công!";

                    return RedirectToAction("Index");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    foreach (var entityError in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in entityError.ValidationErrors)
                        {
                            ModelState.AddModelError("", $"Lỗi tại cột '{validationError.PropertyName}': {validationError.ErrorMessage}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(
                        "",
                        "Lỗi khi thêm tài khoản: " +
                        ex.GetBaseException().Message
                    );
                }
            }

            return View(user);
        }

        // GET: Admin/Users/Edit/duy123
        public ActionResult Edit(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User user = db.Users.Find(username);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        // POST: Admin/Users/Edit/duy123
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "Username,Password,UserRole")] User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = db.Users.Find(user.Username);

                    if (existingUser == null)
                    {
                        return HttpNotFound();
                    }

                    existingUser.Password = user.Password;
                    existingUser.UserRole = user.UserRole.Trim();

                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Cập nhật tài khoản thành công!";

                    return RedirectToAction("Index");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    string errorDetails = "";

                    foreach (var entityError in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in entityError.ValidationErrors)
                        {
                            errorDetails += validationError.PropertyName
                                + ": " + validationError.ErrorMessage + " | ";
                        }
                    }

                    ModelState.AddModelError(
                        "",
                        "Chi tiết lỗi dữ liệu: " + errorDetails
                    );
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(
                        "",
                        "Lỗi khi cập nhật tài khoản: " +
                        ex.GetBaseException().Message
                    );
                }
            }

            return View(user);
        }

        // GET: Admin/Users/Delete/duy123
        public ActionResult Delete(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User user = db.Users.Find(username);

            if (user == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra tài khoản có liên kết với khách hàng không
            int customerCount = db.Customers.Count(c => c.Username == username);

            ViewBag.CustomerCount = customerCount;

            return View(user);
        }

        // POST: Admin/Users/Delete/duy123
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                User user = db.Users.Find(id);

                if (user == null)
                {
                    return HttpNotFound();
                }

                // Kiểm tra liên kết với bảng Customer
                int customerCount = db.Customers.Count(c => c.Username == id);

                ViewBag.CustomerCount = customerCount;

                // Nếu tài khoản đang được khách hàng sử dụng
                if (customerCount > 0)
                {
                    ViewBag.DeleteError =
                        "Không thể xóa tài khoản này vì đang được liên kết với " +
                        customerCount +
                        " thông tin khách hàng. Vui lòng kiểm tra dữ liệu khách hàng trước khi xóa.";

                    return View("Delete", user);
                }

                // Nếu không có liên kết thì xóa tài khoản
                db.Users.Remove(user);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Xóa tài khoản thành công!";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                User user = db.Users.Find(id);

                if (user == null)
                {
                    return HttpNotFound();
                }

                ViewBag.CustomerCount = db.Customers.Count(c => c.Username == id);

                ViewBag.DeleteError =
                    "Không thể xóa tài khoản: " +
                    ex.GetBaseException().Message;

                return View("Delete", user);
            }
        }

        // Giải phóng tài nguyên
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